
namespace CommunityServiceProject.Models
{
    public enum MunicipalServiceRequestNotificationType
    {
        RequestSubmitted = 0,
        RequestApproved = 1,
        RequestRejected = 2,

        InvoiceIssued = 3,
        PaymentRequired = 4,
        PaymentProcessing = 5,
        PaymentSuccessful = 6,
        PaymentFailed = 7,

        ServiceInProgress = 8,
        ServiceCompleted = 9,
        RequestCancelled = 10,

        RefundRequested = 11,
        RefundApproved = 12,
        RefundRejected = 13,
        RefundCompleted = 14
    }
}

