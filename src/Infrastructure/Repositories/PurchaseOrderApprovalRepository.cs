using PurchaseOrderAPI.Domain.Entities;
using PurchaseOrderAPI.Domain.Enums;
using PurchaseOrderAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PurchaseOrderAPI.Infrastructure.Repositories
{
    // Repositorio para acessar dados de aprovações
    public class PurchaseOrderApprovalRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PurchaseOrderApprovalRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Create, adiciona uma nova aprovação
        public async Task AddAsync(PurchaseOrderApproval approval)
        {
            await _dbContext.PurchaseOrderApprovals.AddAsync(approval);
            await _dbContext.SaveChangesAsync();
        }

        // Adiciona múltiplas aprovações
        public async Task AddRangeAsync(List<PurchaseOrderApproval> approvals)
        {
            await _dbContext.PurchaseOrderApprovals.AddRangeAsync(approvals);
            await _dbContext.SaveChangesAsync();
        }

        // Read, busca uma aprovação pelo ID
        public async Task<PurchaseOrderApproval?> GetByIdAsync(Guid id)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Include(a => a.Order)
                .Include(a => a.Approver)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // Retorna todas as aprovações
        public async Task<List<PurchaseOrderApproval>> GetAllAsync()
        {
            return await _dbContext.PurchaseOrderApprovals
                .Include(a => a.Order)
                .Include(a => a.Approver)
                .OrderByDescending(a => a.ActionDate)
                .ToListAsync();
        }

        // Retorna aprovações de um pedido específico
        public async Task<List<PurchaseOrderApproval>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Where(a => a.OrderId == orderId)
                .Include(a => a.Approver)
                .OrderBy(a => a.Sequence)
                .ToListAsync();
        }

        // Retorna aprovações pendentes de um pedido
        public async Task<List<PurchaseOrderApproval>> GetPendingByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Where(a => a.OrderId == orderId && a.Status == ApprovalStatus.Pending)
                .Include(a => a.Approver)
                .OrderBy(a => a.Sequence)
                .ToListAsync();
        }

        // Retorna a próxima aprovação pendente de um pedido
        public async Task<PurchaseOrderApproval?> GetNextPendingApprovalAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Where(a => a.OrderId == orderId && a.Status == ApprovalStatus.Pending)
                .Include(a => a.Approver)
                .OrderBy(a => a.Sequence)
                .FirstOrDefaultAsync();
        }

        // Retorna aprovações feitas por um usuário específico
        public async Task<List<PurchaseOrderApproval>> GetByApproverAsync(Guid approverId)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Where(a => a.ApproverId == approverId)
                .Include(a => a.Order)
                .OrderByDescending(a => a.ActionDate)
                .ToListAsync();
        }

        // Retorna aprovações pendentes para um aprovador
        public async Task<List<PurchaseOrderApproval>> GetPendingForApproverAsync(Guid approverId)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Where(a => a.ApproverId == approverId && a.Status == ApprovalStatus.Pending)
                .Include(a => a.Order)
                .OrderByDescending(a => a.Order.CreatedAt)
                .ToListAsync();
        }

        // Retorna aprovações por status
        public async Task<List<PurchaseOrderApproval>> GetByStatusAsync(ApprovalStatus status)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Where(a => a.Status == status)
                .Include(a => a.Order)
                .Include(a => a.Approver)
                .OrderByDescending(a => a.ActionDate)
                .ToListAsync();
        }

        // Retorna aprovações por função
        public async Task<List<PurchaseOrderApproval>> GetByApproverRoleAsync(UserRole role)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Where(a => a.ApproverRole == role)
                .Include(a => a.Order)
                .Include(a => a.Approver)
                .OrderByDescending(a => a.ActionDate)
                .ToListAsync();
        }

        // Update, atualiza uma aprovação
        public async Task UpdateAsync(PurchaseOrderApproval approval)
        {
            _dbContext.PurchaseOrderApprovals.Update(approval);
            await _dbContext.SaveChangesAsync();
        }

        // Delete, deleta uma aprovação
        public async Task DeleteAsync(Guid id)
        {
            var approval = await GetByIdAsync(id);
            if (approval != null)
            {
                _dbContext.PurchaseOrderApprovals.Remove(approval);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Deleta todas as aprovações de um pedido
        public async Task DeleteByOrderIdAsync(Guid orderId)
        {
            var approvals = await GetByOrderIdAsync(orderId);
            if (approvals.Any())
            {
                _dbContext.PurchaseOrderApprovals.RemoveRange(approvals);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Métodos auxiliares, verifica se uma aprovação existe
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.PurchaseOrderApprovals.AnyAsync(a => a.Id == id);
        }

        // Conta aprovações de um pedido
        public async Task<int> CountByOrderAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Where(a => a.OrderId == orderId)
                .CountAsync();
        }

        // Conta aprovações aprovadas de um pedido
        public async Task<int> CountApprovedByOrderAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderApprovals
                .Where(a => a.OrderId == orderId && a.Status == ApprovalStatus.Approved)
                .CountAsync();
        }

        // Verifica se todas as aprovações foram feitas
        public async Task<bool> AllApprovalsCompleteAsync(Guid orderId)
        {
            var totalApprovals = await CountByOrderAsync(orderId);
            var approvedApprovals = await CountApprovedByOrderAsync(orderId);
            return totalApprovals > 0 && totalApprovals == approvedApprovals;
        }
    }
}