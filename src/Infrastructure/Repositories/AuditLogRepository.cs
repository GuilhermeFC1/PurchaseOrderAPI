using PurchaseOrderAPI.Domain.Entities;
using PurchaseOrderAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PurchaseOrderAPI.Infrastructure.Repositories
{
    // Repositorio para acessar dados de auditoria
    public class AuditLogRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AuditLogRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Create, adiciona um novo registro de auditoria
        public async Task AddAsync(AuditLog auditLog)
        {
            await _dbContext.AuditLogs.AddAsync(auditLog);
            await _dbContext.SaveChangesAsync();
        }

        // Read, busca um registro de auditoria pelo ID
        public async Task<AuditLog?> GetByIdAsync(Guid id)
        {
            return await _dbContext.AuditLogs
                .Include(log => log.Order)
                .Include(log => log.User)
                .FirstOrDefaultAsync(log => log.Id == id);
        }

        // Retorna todos os registros de auditoria
        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _dbContext.AuditLogs
                .Include(log => log.Order)
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        // Retorna histórico de um pedido específico
        public async Task<List<AuditLog>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.AuditLogs
                .Where(log => log.OrderId == orderId)
                .Include(log => log.User)
                .OrderBy(log => log.Timestamp)
                .ToListAsync();
        }

        // Retorna ações de um usuário específico
        public async Task<List<AuditLog>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.AuditLogs
                .Where(log => log.UserId == userId)
                .Include(log => log.Order)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        // Retorna registros de um período específico
        public async Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.AuditLogs
                .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
                .Include(log => log.Order)
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        // Retorna registros de um dia específico
        public async Task<List<AuditLog>> GetByDateAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);
            return await GetByDateRangeAsync(startOfDay, endOfDay);
        }

        // Retorna registros de uma ação específica
        public async Task<List<AuditLog>> GetByActionAsync(string action)
        {
            return await _dbContext.AuditLogs
                .Where(log => log.Action.Contains(action))
                .Include(log => log.Order)
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        // Retorna registros recentes (últimas N horas)
        public async Task<List<AuditLog>> GetRecentAsync(int hoursBack = 24)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hoursBack);
            return await _dbContext.AuditLogs
                .Where(log => log.Timestamp >= cutoffTime)
                .Include(log => log.Order)
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        // Delete, deleta um registro de auditoria
        public async Task DeleteAsync(Guid id)
        {
            var auditLog = await GetByIdAsync(id);
            if (auditLog != null)
            {
                _dbContext.AuditLogs.Remove(auditLog);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Deleta registros de auditoria de um pedido
        public async Task DeleteByOrderIdAsync(Guid orderId)
        {
            var logs = await GetByOrderIdAsync(orderId);
            if (logs.Any())
            {
                _dbContext.AuditLogs.RemoveRange(logs);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Deleta registros de auditoria anteriores a uma data
        public async Task DeleteOlderThanAsync(DateTime cutoffDate)
        {
            var logs = await _dbContext.AuditLogs
                .Where(log => log.Timestamp < cutoffDate)
                .ToListAsync();
            
            if (logs.Any())
            {
                _dbContext.AuditLogs.RemoveRange(logs);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Métodos auxiliares, verifica se um registro existe
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.AuditLogs.AnyAsync(log => log.Id == id);
        }

        // Conta registros de um pedido
        public async Task<int> CountByOrderAsync(Guid orderId)
        {
            return await _dbContext.AuditLogs
                .Where(log => log.OrderId == orderId)
                .CountAsync();
        }

        // Conta registros de um usuário
        public async Task<int> CountByUserAsync(Guid userId)
        {
            return await _dbContext.AuditLogs
                .Where(log => log.UserId == userId)
                .CountAsync();
        }

        // Conta registros totais
        public async Task<int> CountAsync()
        {
            return await _dbContext.AuditLogs.CountAsync();
        }

        // Retorna a última ação de um pedido
        public async Task<AuditLog?> GetLastActionByOrderAsync(Guid orderId)
        {
            return await _dbContext.AuditLogs
                .Where(log => log.OrderId == orderId)
                .Include(log => log.User)
                .OrderByDescending(log => log.Timestamp)
                .FirstOrDefaultAsync();
        }

        // Retorna a primeira ação de um pedido, quando foi criado
        public async Task<AuditLog?> GetFirstActionByOrderAsync(Guid orderId)
        {
            return await _dbContext.AuditLogs
                .Where(log => log.OrderId == orderId)
                .Include(log => log.User)
                .OrderBy(log => log.Timestamp)
                .FirstOrDefaultAsync();
        }
    }
}