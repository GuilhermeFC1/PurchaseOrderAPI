using PurchaseOrderAPI.Domain.Entities;
using PurchaseOrderAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PurchaseOrderAPI.Infrastructure.Repositories
{
    // Repositorio para acessar dados de itens de pedidos
    public class PurchaseOrderItemRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PurchaseOrderItemRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Create, adiciona um novo item
        public async Task AddAsync(PurchaseOrderItem item)
        {
            await _dbContext.PurchaseOrderItems.AddAsync(item);
            await _dbContext.SaveChangesAsync();
        }

        // Adiciona múltiplos itens
        public async Task AddRangeAsync(List<PurchaseOrderItem> items)
        {
            await _dbContext.PurchaseOrderItems.AddRangeAsync(items);
            await _dbContext.SaveChangesAsync();
        }

        // Read, busca um item pelo ID
        public async Task<PurchaseOrderItem?> GetByIdAsync(Guid id)
        {
            return await _dbContext.PurchaseOrderItems
                .Include(item => item.Order)
                .FirstOrDefaultAsync(item => item.Id == id);
        }

        // Retorna todos os itens
        public async Task<List<PurchaseOrderItem>> GetAllAsync()
        {
            return await _dbContext.PurchaseOrderItems
                .Include(item => item.Order)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync();
        }

        // Retorna itens de um pedido específico
        public async Task<List<PurchaseOrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderItems
                .Where(item => item.OrderId == orderId)
                .OrderBy(item => item.CreatedAt)
                .ToListAsync();
        }

        // Retorna a quantidade total de itens de um pedido
        public async Task<int> GetItemCountByOrderAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderItems
                .Where(item => item.OrderId == orderId)
                .CountAsync();
        }

        // Retorna o valor total de um pedido, soma de todos os itens
        public async Task<decimal> GetTotalValueByOrderAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderItems
                .Where(item => item.OrderId == orderId)
                .SumAsync(item => item.Quantity * item.UnitPrice);
        }

        // Update, atualiza um item
        public async Task UpdateAsync(PurchaseOrderItem item)
        {
            _dbContext.PurchaseOrderItems.Update(item);
            await _dbContext.SaveChangesAsync();
        }

        // Delete, deleta um item
        public async Task DeleteAsync(Guid id)
        {
            var item = await GetByIdAsync(id);
            if (item != null)
            {
                _dbContext.PurchaseOrderItems.Remove(item);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Deleta todos os itens de um pedido
        public async Task DeleteByOrderIdAsync(Guid orderId)
        {
            var items = await GetByOrderIdAsync(orderId);
            if (items.Any())
            {
                _dbContext.PurchaseOrderItems.RemoveRange(items);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Métodos auxiliares, verifica se um item existe
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.PurchaseOrderItems.AnyAsync(item => item.Id == id);
        }

        // Conta itens de um pedido
        public async Task<int> CountByOrderAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderItems
                .Where(item => item.OrderId == orderId)
                .CountAsync();
        }

        // Verifica se um pedido tem itens
        public async Task<bool> HasItemsAsync(Guid orderId)
        {
            return await _dbContext.PurchaseOrderItems
                .AnyAsync(item => item.OrderId == orderId);
        }
    }
}