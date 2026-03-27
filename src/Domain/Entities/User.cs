using PurchaseOrderAPI.Domain.Enums;

namespace PurchaseOrderAPI.Domain.Entities
{
    // Classe abstrata que é base para todos os usuários
    public abstract class User
    {
        // Identificador único do usuário
        public Guid Id { get; set; }

        // Nome completo
        public string FullName { get; set; } = string.Empty;

        // Email único
        public string Email { get; set; } = string.Empty;

        // Papel do usuário
        public UserRole UserRole { get; set; }

        // Departamento
        public string Department { get; set; } = string.Empty;

        // Data de criação
        public DateTime CreatedAt { get; set; }

        // Ativo ou inativo
        public bool IsActive { get; set; }

        // Última modificação
        public DateTime? LastModified { get; set; }

        // Relacionamentos
        // Pedidos criados por este usuário
        public virtual ICollection<PurchaseOrder> CreatedPurchaseOrders { get; set; } = new List<PurchaseOrder>();

        // Aprovações realizadas por este usuário
        public virtual ICollection<PurchaseOrderApproval> Approvals { get; set; } = new List<PurchaseOrderApproval>();

        // Ações registradas por este usuário (auditoria)
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // Retorna o papel do usuário
        public virtual UserRole GetRole()
        {
            return UserRole;
        }

        // Verifica se pode aprovar
        public virtual bool IsApprover()
        {
            return UserRole == UserRole.Supplies ||
                   UserRole == UserRole.Manager ||
                   UserRole == UserRole.Director;

        }



    }
}