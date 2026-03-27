namespace PurchaseOrderAPI.Domain.Entities
{
    // Item dentro de um pedido de compra
    public class PurchaseOrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relacionamento
        public virtual PurchaseOrder? Order { get; set; }

        // Calcula subtotal
        public decimal GetSubTotal()
        {
            return Quantity * UnitPrice;
        }

        // Atualiza quantidade
        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity < 1)
                throw new ArgumentException("Quantidade deve ser maior que zero");
            Quantity = newQuantity;
        }

        // Atualiza preço
        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("Preço não pode ser negativo");
            UnitPrice = newPrice;
        }
    }
}