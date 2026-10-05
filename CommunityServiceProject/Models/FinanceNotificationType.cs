namespace CommunityServiceProject.Models
{
    public enum FinanceNotificationType
    {
        InvoiceGenerated,
        PaymentSuccessful,
        PaymentFailed,
        ReceiptGenerated,
        RefundApproved,
        RefundRejected,
        RefundProcessed
    }
}