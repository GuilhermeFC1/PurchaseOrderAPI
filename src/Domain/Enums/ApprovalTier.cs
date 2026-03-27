namespace PurchaseOrderAPI.Domain.Enums
{
    // Quem pode aprovar a venda baseado no valor do pedido
    public enum ApprovalTier
    {
        Tier1 = 1,      // Até R$100,00 - Suprimentos aprova
        Tier2 = 2,      // Entre R$101,00 e R$1000,00 - Suprimentos + Gerente aprovam
        Tier3 = 3       // Acima de R$1000,00 - Suprimentos + Gerente + Diretor aprovam
    }
}