namespace PurchaseOrderAPI.Domain.Enums
{
    // Estados que uma aprovação individual possa ter
    public enum ApprovalStatus
    {
        Pending = 0,            // Aguardando ação
        Approved = 1,           // Aprovado
        Rejected = 2,           // Rejeitado
        RequestRevision = 3,    // Revisão solicitado
        Cancelled = 4           // Cancelado
    }
}