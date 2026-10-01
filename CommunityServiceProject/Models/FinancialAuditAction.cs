namespace CommunityServiceProject.Models
{
    public enum FinancialAuditAction
    {
        Created,
        Updated,
        Approved,
        Rejected,
        Cancelled,
        PaymentProcessed,
        PaymentFailed,
        RefundRequested,
        RefundApproved,
        RefundRejected,
        RefundProcessed,
        PayrollCalculated,
        PayrollApproved,
        PayrollPaid
    }
}