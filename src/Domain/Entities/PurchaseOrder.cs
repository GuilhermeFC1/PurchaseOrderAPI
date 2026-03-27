using PurchaseOrderAPI.Domain.Enums;

namespace PurchaseOrderAPI.Domain.Entities
{
    // Representa um pedido de compra
    public class PurchaseOrder
    {
        public Guid Id { get; set; }
        public Guid ElaboratorId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalValue { get; set; } 
        public OrderStatus Status { get; set; } = OrderStatus.Draft;
        public ApprovalTier ApprovalTier { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Description { get; set; }

        // Relacionamentos
        public virtual User? Elaborator { get; set; }
        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
        public virtual ICollection<PurchaseOrderApproval> Approvals { get; set; } = new List<PurchaseOrderApproval>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        
        // Determina a alçada baseada no valor
        public ApprovalTier DetermineApprovalTier()
        {
            if (TotalValue <= 100)
                return ApprovalTier.Tier1;
            else if (TotalValue <= 1000)
                return ApprovalTier.Tier2;
            else
                return ApprovalTier.Tier3;
        }

        // Adiciona um item
        public void AddItem(PurchaseOrderItem item)
        {
            if (Status != OrderStatus.Draft)
                throw new InvalidOperationException("Apenas pedidos em Draft podem receber itens");
            Items.Add(item);
            TotalValue = CalculateTotalValue();
            ApprovalTier = DetermineApprovalTier();
        }

        // Remove um item
        public void RemoveItem(Guid itemId)
        {
            if (Status != OrderStatus.Draft)
                throw new InvalidOperationException("Apenas pedidos em Draft podem ter itens removidos");
            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                Items.Remove(item);
                TotalValue = CalculateTotalValue();
                ApprovalTier = DetermineApprovalTier();
            }
        }

        // Verifica se todas as aprovações existem
        public bool IsComplete()
        {
            if (Approvals.Count == 0) return false;
            return Approvals.All(a => a.Status == ApprovalStatus.Approved);
        }

        // Retorna a próxima aprovação pendente
        public PurchaseOrderApproval? GetNextPendingApproval()
        {
            return Approvals
                .Where(a => a.Status == ApprovalStatus.Pending)
                .OrderBy(a => a.Sequence)
                .FirstOrDefault();
        }

        // Calcula o valor total somando todos os itens
        private decimal CalculateTotalValue()
        {
            return Items.Sum(i => i.Quantity * i.UnitPrice);
        }

        // Cancela o pedido
        public void Cancel()
        {
            Status = OrderStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}