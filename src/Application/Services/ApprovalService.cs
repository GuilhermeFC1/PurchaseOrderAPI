using PurchaseOrderAPI.Domain.Entities;
using PurchaseOrderAPI.Domain.Enums;
using PurchaseOrderAPI.Infrastructure.Repositories;

namespace PurchaseOrderAPI.Application.Services
{
    // Implementa a lógica de negócio para aprovações
    public class ApprovalService
    {
        // Referências aos repositorios
        private readonly PurchaseOrderApprovalRepository _approvalRepository;
        private readonly PurchaseOrderRepository _orderRepository;
        private readonly AuditLogRepository _auditRepository;
        private readonly UserRepository _userRepository;

        // Construtor que recebe os repositorios
        public ApprovalService(
            PurchaseOrderApprovalRepository approvalRepository,
            PurchaseOrderRepository orderRepository,
            AuditLogRepository auditRepository,
            UserRepository userRepository)
        {
            _approvalRepository = approvalRepository;
            _orderRepository = orderRepository;
            _auditRepository = auditRepository;
            _userRepository = userRepository;
        }

        // Aprova um pedido
        public async Task ApprovePurchaseOrderAsync(Guid orderId, Guid approverId, string comments = "")
        {
            // Valida se pedido existe
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Pedido não encontrado");

            // Valida se pedido está em aprovação
            if (order.Status != OrderStatus.InApproval)
                throw new InvalidOperationException("Pedido não está aguardando aprovação");

            // Valida se aprovador existe
            var approver = await _userRepository.GetByIdAsync(approverId);
            if (approver == null)
                throw new InvalidOperationException("Aprovador não encontrado");

            // Valida se é um aprovador
            if (!approver.IsApprover())
                throw new InvalidOperationException("Usuário não é um aprovador");

            // Busca a próxima aprovação pendente
            var nextApproval = await _approvalRepository.GetNextPendingApprovalAsync(orderId);
            if (nextApproval == null)
                throw new InvalidOperationException("Nenhuma aprovação pendente encontrada");

            // Valida se o aprovador tem o papel correto
            if (nextApproval.ApproverRole != approver.UserRole)
                throw new InvalidOperationException(
                    $"Este pedido aguarda aprovação de {nextApproval.ApproverRole}, não {approver.UserRole}");

            // Aprova a aprovação
            nextApproval.ApproverId = approverId;
            nextApproval.Approve();
            nextApproval.Comments = comments;
            await _approvalRepository.UpdateAsync(nextApproval);

            // Verifica se todas as aprovações estão completas
            var allComplete = await _approvalRepository.AllApprovalsCompleteAsync(orderId);
            if (allComplete)
            {
                // Marca o pedido como aprovado
                order.Status = OrderStatus.Approved;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);
            }

            // Registra na auditoria
            var auditLog = AuditLog.CreateLog(
                orderId,
                approverId,
                $"Pedido aprovado por {approver.UserRole}",
                comments
            );
            await _auditRepository.AddAsync(auditLog);
        }

        // Rejeita um pedido
        public async Task RejectPurchaseOrderAsync(Guid orderId, Guid approverId, string reason)
        {
            // Valida se pedido existe
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Pedido não encontrado");

            // Valida se pedido está em aprovação
            if (order.Status != OrderStatus.InApproval)
                throw new InvalidOperationException("Pedido não está aguardando aprovação");

            // Valida se aprovador existe
            var approver = await _userRepository.GetByIdAsync(approverId);
            if (approver == null)
                throw new InvalidOperationException("Aprovador não encontrado");

            // Valida se tem motivo
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Motivo da rejeição é obrigatório");

            // Busca a aprovação pendente atual
            var nextApproval = await _approvalRepository.GetNextPendingApprovalAsync(orderId);
            if (nextApproval == null)
                throw new InvalidOperationException("Nenhuma aprovação pendente encontrada");

            // Rejeita a aprovação
            nextApproval.ApproverId = approverId;
            nextApproval.Reject(reason);
            await _approvalRepository.UpdateAsync(nextApproval);

            // Marca o pedido como rejeitado
            order.Status = OrderStatus.Rejected;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            // Registra na auditoria
            var auditLog = AuditLog.CreateLog(
                orderId,
                approverId,
                $"Pedido rejeitado por {approver.UserRole}",
                reason
            );
            await _auditRepository.AddAsync(auditLog);
        }


        // Solicita revisão de um pedido
        public async Task RequestRevisionAsync(Guid orderId, Guid approverId, string reason)
        {
            // Valida se pedido existe
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Pedido não encontrado");

            // Valida se pedido está em aprovação
            if (order.Status != OrderStatus.InApproval)
                throw new InvalidOperationException("Pedido não está aguardando aprovação");

            // Valida se aprovador existe
            var approver = await _userRepository.GetByIdAsync(approverId);
            if (approver == null)
                throw new InvalidOperationException("Aprovador não encontrado");

            // Valida se tem motivo
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Motivo da revisão é obrigatório");

            // Busca a aprovação pendente atual
            var nextApproval = await _approvalRepository.GetNextPendingApprovalAsync(orderId);
            if (nextApproval == null)
                throw new InvalidOperationException("Nenhuma aprovação pendente encontrada");

            // Solicita revisão
            nextApproval.ApproverId = approverId;
            nextApproval.RequestRevision(reason);
            await _approvalRepository.UpdateAsync(nextApproval);

            // Volta pedido para Draft
            order.Status = OrderStatus.Draft;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            // Cancela as demais aprovações
            var otherApprovals = await _approvalRepository.GetByOrderIdAsync(orderId);
            foreach (var approval in otherApprovals.Where(a => a.Id != nextApproval.Id))
            {
                approval.Status = ApprovalStatus.Cancelled;
                await _approvalRepository.UpdateAsync(approval);
            }

            // Registra na auditoria
            var auditLog = AuditLog.CreateLog(
                orderId,
                approverId,
                $"Revisão solicitada por {approver.UserRole}",
                reason
            );
            await _auditRepository.AddAsync(auditLog);
        }


        // Retorna as aprovações de um pedido
        public async Task<List<PurchaseOrderApproval>> GetApprovalChainAsync(Guid orderId)
        {
            return await _approvalRepository.GetByOrderIdAsync(orderId);
        }

        // Retorna a próxima aprovação pendente
        public async Task<PurchaseOrderApproval?> GetNextPendingApprovalAsync(Guid orderId)
        {
            return await _approvalRepository.GetNextPendingApprovalAsync(orderId);
        }

        // Retorna aprovações pendentes para um aprovador
        public async Task<List<PurchaseOrderApproval>> GetPendingApprovalsForUserAsync(Guid approverId)
        {
            return await _approvalRepository.GetPendingForApproverAsync(approverId);
        }

        // Retorna o número de aprovações pendentes para um usuário
        public async Task<int> GetPendingApprovalsCountAsync(Guid approverId)
        {
            var pendingApprovals = await _approvalRepository.GetPendingForApproverAsync(approverId);
            return pendingApprovals.Count;
        }

        // Métodos auxiliares, verifica se um pedido tem todas as aprovações completas
        public async Task<bool> IsApprovalCompleteAsync(Guid orderId)
        {
            return await _approvalRepository.AllApprovalsCompleteAsync(orderId);
        }

        // Retorna quantas aprovações faltam
        public async Task<int> GetRemainingApprovalsAsync(Guid orderId)
        {
            var approvals = await _approvalRepository.GetByOrderIdAsync(orderId);
            return approvals.Count(a => a.Status == ApprovalStatus.Pending);
        }

        // Retorna o progresso de aprovação em percentual
        public async Task<int> GetApprovalProgressPercentageAsync(Guid orderId)
        {
            var approvals = await _approvalRepository.GetByOrderIdAsync(orderId);
            if (approvals.Count == 0) return 0;

            var approvedCount = approvals.Count(a => a.Status == ApprovalStatus.Approved);
            return (approvedCount * 100) / approvals.Count;
        }
    }
}