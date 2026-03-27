using PurchaseOrderAPI.Domain.Entities;
using PurchaseOrderAPI.Domain.Enums;
using PurchaseOrderAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PurchaseOrderAPI.Infrastructure.Repositories
{
    // Repositorio para acessar dados de usuários
    public class UserRepository
    {
        private readonly ApplicationDbContext _dbContext;
 
        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
 
        // Create, adiciona um novo usuário
        public async Task AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }
 
        // Read, busca um usuário pelo ID
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Users
                .Include(u => u.CreatedPurchaseOrders)
                .Include(u => u.Approvals)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
 
        // Busca um usuário pelo email
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }
 
        // Retorna todos os usuários
        public async Task<List<User>> GetAllAsync()
        {
            return await _dbContext.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }
 
        // Retorna usuários por função
        public async Task<List<User>> GetByRoleAsync(UserRole role)
        {
            return await _dbContext.Users
                .Where(u => u.UserRole == role)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }
 
        // Retorna apenas os usuários ativos
        public async Task<List<User>> GetActiveAsync()
        {
            return await _dbContext.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }
 
        // Retorna usuários aprovadores
        public async Task<List<User>> GetApproversAsync()
        {
            return await _dbContext.Users
                .Where(u => u.IsActive && u.UserRole != UserRole.Elaborator)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }
 
        // Retorna usuários por departamento
        public async Task<List<User>> GetByDepartmentAsync(string department)
        {
            return await _dbContext.Users
                .Where(u => u.Department == department && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }
 
        // Update, atualiza um usuário
        public async Task UpdateAsync(User user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
 
        // Delete, deleta um usuário
        public async Task DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
        }
 
        // Métodos auxiliares, verifica se um usuário existe
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Users.AnyAsync(u => u.Id == id);
        }
 
        // Verifica se um email já existe
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == email);
        }
 
        // Conta quantos usuários existem
        public async Task<int> CountAsync()
        {
            return await _dbContext.Users.CountAsync();
        }
 
        // Conta usuários por papel
        public async Task<int> CountByRoleAsync(UserRole role)
        {
            return await _dbContext.Users
                .Where(u => u.UserRole == role)
                .CountAsync();
        }
    }
}