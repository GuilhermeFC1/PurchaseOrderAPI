using PurchaseOrderAPI.Domain.Entities;
using PurchaseOrderAPI.Domain.Enums;
using PurchaseOrderAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PurchaseOrderAPI.Infrastructure.Repositories
{
    // Repositorio para acessar dados de pedidos de compras
    public class PurchaseOrderRepository
    {
        // Referencia ao DbContext, acesso ao banco de dados
        private readonly ApplicationDbContext _dbContext;

        // Construtor que recebe o DbContext
        public PurchaseOrderRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Create, adiciona um novo pedido ao banco
        public async Task AddAsync(PurchaseOrder order)
        {
            await _dbContext.PurchaseOrders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
        }

        // Read, busca um pedido pelo ID
        public async Task<PurchaseOrder?> GetByIdAsync(Guid id)
        {
            return await _dbContext.PurchaseOrders
                .Include(po => po.Items)            // Inclui os itens
                .Include(po => po.Approvals)        // Inclui as aprovações
                .Include(po => po.AuditLogs)        // Inclui o histórico
                .Include(po => po.Elaborator)       // Inclui o usuário que criou
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        // Busca um pedido pelo número do pedido
        public async Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _dbContext.PurchaseOrders
                .Include(po => po.Items)
                .Include(po => po.Approvals)
                .FirstOrDefaultAsync(po => po.OrderNumber == orderNumber);
        }

        // Retorna todos os pedidos
        public async Task<List<PurchaseOrder>> GetAllAsync()
        {
            return await _dbContext.PurchaseOrders
                .Include(po => po.Items)
                .Include(po => po.Approvals)
                .Include(po => po.Elaborator)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        // Retorna pedidos de um usuário específico
        public async Task<List<PurchaseOrder>> GetByElaboratorAsync(Guid elaboratorId)
        {
            return await _dbContext.PurchaseOrders
                .Where(po => po.ElaboratorId == elaboratorId)
                .Include(po => po.Items)
                .Include(po => po.Approvals)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        // Retorna pedidos por status
        public async Task<List<PurchaseOrder>> GetByStatusAsync(OrderStatus status)
        {
            return await _dbContext.PurchaseOrders
                .Where(po => po.Status == status)
                .Include(po => po.Items)
                .Include(po => po.Approvals)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        // Retorna pedidos por alçada (Tier1, Tier2, Tier3)
        public async Task<List<PurchaseOrder>> GetByApprovalTierAsync(ApprovalTier tier)
        {
            return await _dbContext.PurchaseOrders
                .Where(po => po.ApprovalTier == tier)
                .Include(po => po.Items)
                .Include(po => po.Approvals)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }
 
        // Retorna pedidos aguardando aprovação
        public async Task<List<PurchaseOrder>> GetPendingApprovalsAsync()
        {
            return await _dbContext.PurchaseOrders
                .Where(po => po.Status == OrderStatus.InApproval)
                .Include(po => po.Items)
                .Include(po => po.Approvals)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        // Update, atualiza um pedido existente
        public async Task UpdateAsync(PurchaseOrder order)
        {
            _dbContext.PurchaseOrders.Update(order);
            await _dbContext.SaveChangesAsync();
        }

        // Delete, deleta um pedido pelo ID
        public async Task DeleteAsync(Guid id)
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                _dbContext.PurchaseOrders.Remove(order);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Métodos auxiliares, verifica se um pedido existe
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.PurchaseOrders.AnyAsync(po => po.Id == id);
        }

        // Conta quantos pedidos existem
        public async Task<int> CountAsync()
        {
            return await _dbContext.PurchaseOrders.CountAsync();
        }

        // Conta pedidos por status
        public async Task<int> CountByStatusAsync(OrderStatus status)
        {
            return await _dbContext.PurchaseOrders
                .Where(po => po.Status == status)
                .CountAsync();
        }

    }
}