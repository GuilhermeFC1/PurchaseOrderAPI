namespace PurchaseOrderAPI.Domain.Enums
{
    // Estados que um pedido pode ter durante a venda
    public enum OrderStatus
    {
        Draft = 0,          // Rascunho
        InApproval = 1,     // Aguardando aprovações
        Approved = 2,       // Aprovado por todos
        Rejected = 3,       // Rejeitado
        Cancelled = 4       // Cancelado
    }
}