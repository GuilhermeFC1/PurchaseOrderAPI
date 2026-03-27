using PurchaseOrderAPI.Domain.Entities;
using PurchaseOrderAPI.Infrastructure.Repositories;

namespace PurchaseOrderAPI.Application.Services
{
    // Implementa a lógica de negócio para auditoria
    public class AuditService
    {
        // Referência ao repositorio
        private readonly AuditLogRepository _auditRepository;

        // Construtor que recebe o repositorio
        public AuditService(AuditLogRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        // Registra uma ação no histórico
        public async Task LogActionAsync(Guid orderId, Guid userId, string action, string? details = null)
        {
            // Valida se os IDs são válidos
            if (orderId == Guid.Empty)
                throw new ArgumentException("ID do pedido é obrigatório");

            if (userId == Guid.Empty)
                throw new ArgumentException("ID do usuário é obrigatório");

            // Valida se a ação não está vazia
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Ação é obrigatória");

            // Cria o registro de auditoria
            var auditLog = AuditLog.CreateLog(orderId, userId, action, details);

            // Salva no banco
            await _auditRepository.AddAsync(auditLog);
        }


        // Retorna o histórico completo de um pedido
        public async Task<List<AuditLog>> GetOrderHistoryAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("ID do pedido é obrigatório");

            return await _auditRepository.GetByOrderIdAsync(orderId);
        }

        // Retorna as ações de um usuário
        public async Task<List<AuditLog>> GetUserActionsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("ID do usuário é obrigatório");

            return await _auditRepository.GetByUserIdAsync(userId);
        }

        // Retorna registros de um período
        public async Task<List<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Valida se as datas estão em ordem
            if (startDate > endDate)
                throw new ArgumentException("Data inicial deve ser menor que data final");

            return await _auditRepository.GetByDateRangeAsync(startDate, endDate);
        }

        // Retorna registros de um dia específico
        public async Task<List<AuditLog>> GetAuditLogsByDateAsync(DateTime date)
        {
            return await _auditRepository.GetByDateAsync(date);
        }

        // Retorna registros de uma ação específica
        public async Task<List<AuditLog>> GetAuditLogsByActionAsync(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Ação é obrigatória");

            return await _auditRepository.GetByActionAsync(action);
        }

        // Retorna registros recentes
        public async Task<List<AuditLog>> GetRecentAuditLogsAsync(int hoursBack = 24)
        {
            if (hoursBack <= 0)
                throw new ArgumentException("Horas deve ser maior que zero");

            return await _auditRepository.GetRecentAsync(hoursBack);
        }


        // Retorna um relatório resumido do histórico de um pedido
        public async Task<AuditReportDto> GetOrderAuditReportAsync(Guid orderId)
        {
            var auditLogs = await GetOrderHistoryAsync(orderId);

            // Encontra primeira e última ação
            var firstAction = auditLogs.OrderBy(log => log.Timestamp).FirstOrDefault();
            var lastAction = auditLogs.OrderByDescending(log => log.Timestamp).FirstOrDefault();

            return new AuditReportDto
            {
                OrderId = orderId,
                TotalActions = auditLogs.Count,
                FirstActionTime = firstAction?.Timestamp,
                LastActionTime = lastAction?.Timestamp,
                ActionsPerformed = auditLogs.Select(log => log.Action).Distinct().ToList(),
                UsersInvolved = auditLogs.Select(log => log.UserId).Distinct().Count(),
                Logs = auditLogs
            };
        }

        // Retorna estatísticas de auditoria
        public async Task<AuditStatisticsDto> GetAuditStatisticsAsync()
        {
            var totalLogs = await _auditRepository.CountAsync();
            var recentLogs = await _auditRepository.GetRecentAsync(24);

            return new AuditStatisticsDto
            {
                TotalAuditLogs = totalLogs,
                AuditLogsLast24Hours = recentLogs.Count,
                MostCommonAction = recentLogs
                    .GroupBy(log => log.Action)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "Nenhuma"
            };
        }


        // Limpa registros de auditoria antigos
        public async Task DeleteOldAuditLogsAsync(int daysOld)
        {
            if (daysOld <= 0)
                throw new ArgumentException("Dias deve ser maior que zero");

            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            await _auditRepository.DeleteOlderThanAsync(cutoffDate);
        }

        // Retorna informações sobre um registro de auditoria específico
        public async Task<AuditLog?> GetAuditLogDetailAsync(Guid auditLogId)
        {
            return await _auditRepository.GetByIdAsync(auditLogId);
        }

        // Exporta histórico de um pedido para análise
        public async Task<string> ExportOrderHistoryAsync(Guid orderId)
        {
            var auditLogs = await GetOrderHistoryAsync(orderId);

            if (!auditLogs.Any())
                return "Nenhum histórico encontrado para este pedido.";

            var report = new System.Text.StringBuilder();
            report.AppendLine($"=== HISTÓRICO DO PEDIDO {orderId} ===");
            report.AppendLine($"Total de ações: {auditLogs.Count}");
            report.AppendLine();

            foreach (var log in auditLogs.OrderBy(l => l.Timestamp))
            {
                report.AppendLine($"[{log.Timestamp:dd/MM/yyyy HH:mm:ss}] {log.Action}");
                if (!string.IsNullOrEmpty(log.Details))
                    report.AppendLine($"  Detalhes: {log.Details}");
                report.AppendLine();
            }

            return report.ToString();
        }
    }

    // DTO para Relatório de Auditoria de Pedido
    public class AuditReportDto
    {
        public Guid OrderId { get; set; }
        public int TotalActions { get; set; }
        public DateTime? FirstActionTime { get; set; }
        public DateTime? LastActionTime { get; set; }
        public List<string> ActionsPerformed { get; set; } = new();
        public int UsersInvolved { get; set; }
        public List<AuditLog> Logs { get; set; } = new();
    }

    // DTO para Estatísticas de Auditoria
    public class AuditStatisticsDto
    {
        public int TotalAuditLogs { get; set; }
        public int AuditLogsLast24Hours { get; set; }
        public string MostCommonAction { get; set; } = string.Empty;
    }
}