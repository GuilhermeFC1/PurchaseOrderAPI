using PurchaseOrderAPI.Domain.Enums;

namespace PurchaseOrderAPI.Domain.Entities
{
    // Gerente que aprova pedidos.
    public class Manager : User
    {
        public Manager()
        {
            UserRole = UserRole.Manager;
        }

        public void ApprovePurchaseOrder(PurchaseOrderApproval approval)
        {
            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException("Somente aprovações pendentes podem ser aprovadas");

            approval.Status = ApprovalStatus.Approved;
            approval.ActionDate = DateTime.UtcNow;
        }

        public void RejectPurchaseOrder(PurchaseOrderApproval approval, string reason)
        {
            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException("Somente aprovações pendentes podem ser rejeitadas");

            approval.Status = ApprovalStatus.Rejected;
            approval.ActionDate = DateTime.UtcNow;
        }

        public void RequestRevision(PurchaseOrderApproval approval, string reason)
        {
            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException("Somente aprovações pendentes podem solicitar revisão");

            approval.Status = ApprovalStatus.RequestRevision;
            approval.ActionDate = DateTime.UtcNow;
        }

        public void CancelOrder(PurchaseOrder order, string reason)
        {
            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Pedido já foi cancelado");

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
        }
    }
}