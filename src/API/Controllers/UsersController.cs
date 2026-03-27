using Microsoft.AspNetCore.Mvc;
using PurchaseOrderAPI.Application.Services;
using PurchaseOrderAPI.Domain.Entities;
using PurchaseOrderAPI.Domain.Enums;
using PurchaseOrderAPI.Infrastructure.Repositories;

namespace PurchaseOrderAPI.API.Controllers
{
    // Controller que gerencia todos os endpoints relacionados a usuários
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        // Referência ao repositorio
        private readonly UserRepository _userRepository;

        // Construtor que recebe o repositorio
        public UsersController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Create (POST) cria um novo usuário
        // POST /api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                // Valida se os dados foram fornecidos
                if (string.IsNullOrWhiteSpace(dto.FullName))
                    return BadRequest("Nome completo é obrigatório");

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest("Email é obrigatório");

                if (string.IsNullOrWhiteSpace(dto.Department))
                    return BadRequest("Departamento é obrigatório");

                // Verifica se email já existe
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null)
                    return BadRequest("Email já cadastrado");

                // Cria o usuário apropriado baseado no papel
                User user = CreateUserByRole(dto.UserRole, dto.FullName, dto.Email, dto.Department);

                // Salva no banco
                await _userRepository.AddAsync(user);

                return Created($"/api/users/{user.Id}", new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    userRole = user.UserRole,
                    department = user.Department
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Read (GET) retorna todos os usuários
        // GET /api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();

                return Ok(new
                {
                    success = true,
                    count = users.Count,
                    data = users.Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        email = u.Email,
                        userRole = u.UserRole,
                        department = u.Department,
                        isActive = u.IsActive
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna um usuário específico pelo ID
        // GET /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = user.Id,
                        fullName = user.FullName,
                        email = user.Email,
                        userRole = user.UserRole,
                        department = user.Department,
                        isActive = user.IsActive,
                        createdAt = user.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna usuários por função
        // GET /api/users/role/{role}
        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            try
            {
                // Converte string para enum
                if (!Enum.TryParse<UserRole>(role, true, out var userRole))
                    return BadRequest("Papel inválido");

                var users = await _userRepository.GetByRoleAsync(userRole);

                return Ok(new
                {
                    success = true,
                    count = users.Count,
                    data = users.Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        email = u.Email,
                        userRole = u.UserRole,
                        department = u.Department
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna apenas usuários ativos
        // GET /api/users/active/list
        [HttpGet("active/list")]
        public async Task<IActionResult> GetActiveUsers()
        {
            try
            {
                var users = await _userRepository.GetActiveAsync();

                return Ok(new
                {
                    success = true,
                    count = users.Count,
                    data = users.Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        email = u.Email,
                        userRole = u.UserRole,
                        department = u.Department
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna usuários que são aprovadores
        // GET /api/users/approvers/list
        [HttpGet("approvers/list")]
        public async Task<IActionResult> GetApprovers()
        {
            try
            {
                var approvers = await _userRepository.GetApproversAsync();

                return Ok(new
                {
                    success = true,
                    count = approvers.Count,
                    data = approvers.Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        email = u.Email,
                        userRole = u.UserRole,
                        department = u.Department
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Retorna usuários por departamento
        // GET /api/users/department/{department}
        [HttpGet("department/{department}")]
        public async Task<IActionResult> GetUsersByDepartment(string department)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(department))
                    return BadRequest("Departamento é obrigatório");

                var users = await _userRepository.GetByDepartmentAsync(department);

                return Ok(new
                {
                    success = true,
                    count = users.Count,
                    data = users.Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        email = u.Email,
                        userRole = u.UserRole,
                        department = u.Department
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Update (PUT) atualiza um usuário
        // PUT /api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

                // Atualiza apenas campos permitidos
                if (!string.IsNullOrWhiteSpace(dto.FullName))
                    user.FullName = dto.FullName;

                if (!string.IsNullOrWhiteSpace(dto.Department))
                    user.Department = dto.Department;

                user.LastModified = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                return Ok(new
                {
                    success = true,
                    message = "Usuário atualizado com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Ativa ou desativa um usuário
        // PUT /api/users/{id}/toggle-active
        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

                // Alterna o status de ativo
                user.IsActive = !user.IsActive;
                user.LastModified = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                return Ok(new
                {
                    success = true,
                    message = $"Usuário {(user.IsActive ? "ativado" : "desativado")} com sucesso",
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Deleta um usuário
        // DELETE /api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                    return NotFound(new { success = false, message = "Usuário não encontrado" });

                await _userRepository.DeleteAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Usuário deletado com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Métodos auxiliares, cria o usuário apropriado baseado na função
        private User CreateUserByRole(UserRole role, string fullName, string email, string department)
        {
            User user = role switch
            {
                UserRole.Elaborator => new Elaborator
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Email = email,
                    Department = department,
                    UserRole = UserRole.Elaborator,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                UserRole.Supplies => new SuppliesTeam
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Email = email,
                    Department = department,
                    UserRole = UserRole.Supplies,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                UserRole.Manager => new Manager
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Email = email,
                    Department = department,
                    UserRole = UserRole.Manager,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                UserRole.Director => new Director
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Email = email,
                    Department = department,
                    UserRole = UserRole.Director,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                _ => throw new ArgumentException("Papel inválido")
            };

            return user;
        }
    }

    // DTO para criar usuário
    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole UserRole { get; set; }
        public string Department { get; set; } = string.Empty;
    }

    // DTO para atualizar usuário
    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Department { get; set; }
    }
}