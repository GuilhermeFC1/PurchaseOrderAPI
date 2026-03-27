using PurchaseOrderAPI.Domain.Enums;

namespace PurchaseOrderAPI.Domain.Entities
{
    // Registro de uma aprovação
    public class PurchaseOrderApproval
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid? ApproverId { get; set; }
        public UserRole ApproverRole { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public int Sequence { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? Comments { get; set; }

        // Relacionamentos
        public virtual PurchaseOrder? Order { get; set; }
        public virtual User? Approver { get; set; }

        // Aprova
        public void Approve()
        {
            Status = ApprovalStatus.Approved;
            ActionDate = DateTime.UtcNow;
        }

        // Rejeita
        public void Reject(string reason)
        {
            Status = ApprovalStatus.Rejected;
            Comments = reason;
            ActionDate = DateTime.UtcNow;
        }

        // Solicita revisão
        public void RequestRevision(string reason)
        {
            Status = ApprovalStatus.RequestRevision;
            Comments = reason;
            ActionDate = DateTime.UtcNow;
        }

        // Verifica se está pendente
        public bool IsApprovalPending()
        {
            return Status == ApprovalStatus.Pending;
        }
    }
}