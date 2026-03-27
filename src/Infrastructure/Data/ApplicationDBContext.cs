using Microsoft.EntityFrameworkCore;
using PurchaseOrderAPI.Domain.Entities;
namespace PurchaseOrderAPI.Infrastructure.Data

// A classe "ApplicationDbContext" será responsável por gerenciar a conexão com o SQL Server,
// mapear as entidades c# para as tabelas do banco, definir os relacionamentos entre as tabelas,
// executar CRUD e gerenciar o versionamento do banco.
{
    public class ApplicationDbContext: DbContext
    {
        // DbContextOptions contém as informações de qual banco de dados usar,
        // qual o endereço do banco e mais configurações específicas do banco. 
        // O ": base(options)" passa as opções para a classe pai "DbContext", permitindo
        // que o EF Core saiba se conectar ao banco.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        // Dbsets, tabelas do banco de dados
        public DbSet<User> Users { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PurchaseOrderApproval> PurchaseOrderApprovals { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }


        // Este método define como as classes c# viram tabelas no banco de dados, define
        // primary keys, foreign keys, validações, etc...
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuração da tabela "Users", ele herda outras classes (Elaborator,
            // Manager, Director, SuppliesTeam). "HasDiscriminator" cria uma coluna 
            // que diferencia o tipo de usuário, isso é chamado table per hierarchy (TPH),
            // é uma estratégia que armazena todas as hierarquias em uma única tabela no
            // banco de dados.
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Elaborator>("Elaborator")
                .HasValue<SuppliesTeam>("SuppliesTeam")
                .HasValue<Manager>("Manager")
                .HasValue<Director>("Director");
            
            // Define que Id é uma primary key.
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
            
            // Define que o email é obrigatório e tem máximo de 255 caracteres.
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            // Define que o email é único, 2 usuários não podem ter o mesmo email.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            // Define que o nome completo é obrigatório e tem no máximo 60 caracteres.
            modelBuilder.Entity<User>()
                .Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(60);
            
            // Define que o departamento do usuário é obrigatório e tem no máximo 100 caracteres. 
            modelBuilder.Entity<User>()
                .Property(u => u.Department)
                .IsRequired()
                .HasMaxLength(100);

            
            // Configuração da tabela "PurchaseOrders"

            // Define que Id é primary key
            modelBuilder.Entity<PurchaseOrder>()
                .HasKey(po => po.Id);
            
            // Define que o OrderNumber (número do pedido) é obrigatório, máximo 50 caracteres.
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);
            
            // Define que o OrderNumber deve ser único, cada pedido tem o seu próprio.
            modelBuilder.Entity<PurchaseOrder>()
                .HasIndex(po => po.OrderNumber)
                .IsUnique();
            
            // Define que o TotalValue tem até 18 digitos no total, sendo 2 casas decimais.
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.TotalValue)
                .HasPrecision(18, 2);
            
            // Define que o status do pedido é obrigatorio.
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.Status)
                .IsRequired();
            
            // Define que o ApprovalTier é obrigatório.
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.ApprovalTier)
                .IsRequired();
            
            // 1 User (elaborador) pode criar multiplos "PurchaseOrders".
            // 1 PurchaseOrder é criado somente por 1 "User".
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Elaborator)
                .WithMany(u => u.CreatedPurchaseOrders)
                .HasForeignKey(po => po.ElaboratorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1 PurchaseOrder pode ter multiplos "PurchaseOrderItems".
            // 1 PurchaseOrderItem pertence somente a 1 "PurchaseOrder".
            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(po => po.Items)
                .WithOne(item => item.Order)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1 PurchaseOrder pode ter multiplos "PurchaseOrderApprovals".
            // 1 PurchaseOrderApproval pertence somente a 1 "PurchaseOrder".
            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(po => po.Approvals)
                .WithOne(approval => approval.Order)
                .HasForeignKey(approval => approval.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1 PurchaseOrder pode ter multiplos "AuditLogs".
            // 1 AuditLog pertence somente a 1 "PurchaseOrder".
            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(po => po.AuditLogs)
                .WithOne(audit => audit.Order)
                .HasForeignKey(audit => audit.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Define que Id é primary key.
            modelBuilder.Entity<PurchaseOrderItem>()
                .HasKey(item => item.Id);
            
            // Define que a descrição do item é obrigatória, máximo de 255 caracteres.
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(item => item.Description)
                .IsRequired()
                .HasMaxLength(255);

            // Define que a quantidade é obrigatória.
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(item => item.Quantity)
                .IsRequired();
            
            // Define o valor unitário até 18 dígitos, sendo 2 casas decimais.
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            // Cria um índice em OrderId para otimizar as buscas.
            modelBuilder.Entity<PurchaseOrderItem>()
                .HasIndex(approval => approval.Id);

            
            // Define que Id é a primary key.
            modelBuilder.Entity<PurchaseOrderApproval>()
                .HasKey(approval => approval.Id);
            
            // Define que o "ApprovalRole" é obrigatório.
            modelBuilder.Entity<PurchaseOrderApproval>()
                .Property(approval => approval.ApproverRole)
                .IsRequired();

            // Define que o status é obrigatório.
            modelBuilder.Entity<PurchaseOrderApproval>()
                .Property(approval => approval.Status)
                .IsRequired();

            // Define que a ordem da aprovação seja obrigatória.
            modelBuilder.Entity<PurchaseOrderApproval>()
                .Property(approval => approval.Sequence)
                .IsRequired();
            
            // Comentários sobre motivos de rejeição são opcionais, máximo 1000 caracteres.
            modelBuilder.Entity<PurchaseOrderApproval>()
                .Property(approval => approval.Comments)
                .HasMaxLength(1000);

            // 1 Aprovador pode fazer multiplas "PurchaseOrderApprovals".
            // 1 PurchaseOrderApproval é feita somente por 1 Aprovador (User).
            modelBuilder.Entity<PurchaseOrderApproval>()
                .HasOne(approval => approval.Approver)
                .WithMany(u => u.Approvals)
                .HasForeignKey(approval => approval.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cria índices para otimizar as buscas.
            modelBuilder.Entity<PurchaseOrderApproval>()
                .HasIndex(approval => approval.OrderId);
 
            modelBuilder.Entity<PurchaseOrderApproval>()
                .HasIndex(approval => approval.ApproverId);

            
            // Define que Id é a primary key.
            modelBuilder.Entity<AuditLog>()
                .HasKey(audit => audit.Id);
            
            // Define que a descrição da ação realizada é obrigatória, máximo 255 caracteres.
            modelBuilder.Entity<AuditLog>()
                .Property(audit => audit.Action)
                .IsRequired()
                .HasMaxLength(255);
            
            // Define que os detalhes adicionais são opcionais, máximo 1000 caracteres.
            modelBuilder.Entity<AuditLog>()
                .Property(audit => audit.Details)
                .HasMaxLength(1000);
            

            // 1 User pode realizar multiplos "AuditLogs"
            // 1 AuditLog é realizado somente por 1 "User"
            modelBuilder.Entity<AuditLog>()
                .HasOne(audit => audit.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(audit => audit.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Cria índices para otimizar as buscas.
            modelBuilder.Entity<AuditLog>()
                .HasIndex(audit => audit.OrderId);
 
            modelBuilder.Entity<AuditLog>()
                .HasIndex(audit => audit.UserId);          
            


        }

    }
}