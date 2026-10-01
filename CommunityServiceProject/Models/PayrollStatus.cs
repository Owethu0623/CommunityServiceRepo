namespace CommunityServiceProject.Models
{
    public enum PayrollStatus
    {
        Draft,
        Calculated,
        UnderReview,
        Returned,
        Approved,
        PaymentProcessing,
        PaymentFailed,
        Paid,
        Closed
    }
}