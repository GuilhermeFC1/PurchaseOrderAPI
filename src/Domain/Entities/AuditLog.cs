namespace PurchaseOrderAPI.Domain.Entities
{
    // Registro de auditoria (histórico)
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }

        // Relacionamentos
        public virtual PurchaseOrder? Order { get; set; }
        public virtual User? User { get; set; }

        // Cria um novo log
        public static AuditLog CreateLog(Guid orderId, Guid userId, string action, string? details = null)
        {
            return new AuditLog
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                Details = details
            };
        }

        // String para exibição
        public override string ToString()
        {
            return $"[{Timestamp:dd/MM/yyyy HH:mm:ss}] {Action} - {Details ?? ""}";
        }
    }
}