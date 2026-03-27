using PurchaseOrderAPI.Domain.Enums;

namespace PurchaseOrderAPI.Domain.Entities
{
    // Usuário que cria pedidos de compra.
    public class Elaborator : User
    {
        public Elaborator()
        {
            UserRole = UserRole.Elaborator;
        }

        // Cria um novo pedido.
        public PurchaseOrder CreatePurchaseOrder(string orderNumber, string description = "")
        {
            var order = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                ElaboratorId = this.Id,
                OrderNumber = orderNumber,
                Status = OrderStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                Description = description
            };
            return order;
        }

        // Edita um pedido apenas se estiver em rascunho.
        public void EditPurchaseOrder(PurchaseOrder order, string description)
        {
            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Apenas pedidos em Draft podem ser editados");

            order.Description = description;
            order.UpdatedAt = DateTime.UtcNow;
        }

        // Submete pedido para aprovação.
        public void SubmitForApproval(PurchaseOrder order)
        {
            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Apenas pedidos em Draft podem ser submetidos");

            order.Status = OrderStatus.InApproval;
            order.UpdatedAt = DateTime.UtcNow;
        }
    }
}