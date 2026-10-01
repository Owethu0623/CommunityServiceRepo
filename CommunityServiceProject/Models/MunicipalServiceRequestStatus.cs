namespace CommunityServiceProject.Models
{
    public enum MunicipalServiceRequestStatus
    {
        Submitted,
        UnderReview,
        Approved,
        Rejected,
        InvoiceIssued,
        PaymentRequired,
        PaymentProcessing,
        PaymentFailed,
        Paid,
        ServiceInProgress,
        Completed,
        Cancelled
    }
}