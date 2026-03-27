using PurchaseOrderAPI.Domain.Enums;

namespace PurchaseOrderAPI.Domain.Entities
{
    // Suprimentos que aprova os pedidos.
    public class SuppliesTeam : User
    {
        public SuppliesTeam()
        {
            UserRole = UserRole.Supplies;
        }

        // Aprova um pedido.
        public void ApprovePurchaseOrder(PurchaseOrderApproval approval)
        {
            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException("Somente aprovações pendentes podem ser aprovadas!");

            approval.Status = ApprovalStatus.Approved;
            approval.ActionDate = DateTime.UtcNow;
        }

        // Rejeita um pedido.
        public void RejectPurchaseOrder(PurchaseOrderApproval approval, string reason)
        {
            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException("Somente aprovações pendentes podem ser rejeitadas");

            approval.Status = ApprovalStatus.RequestRevision;
            approval.ActionDate = DateTime.UtcNow;
        }

        // Cancela um pedido.
        public void CancelOrder(PurchaseOrder order, string reason)
        {
            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Pedido já foi cancelado");

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
        }
    }
}