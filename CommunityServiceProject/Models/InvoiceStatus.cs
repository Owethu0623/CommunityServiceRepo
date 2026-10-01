namespace CommunityServiceProject.Models
{
    public enum InvoiceStatus
    {
        Draft,
        Issued,
        PartiallyPaid,
        Paid,
        Overdue,
        Cancelled,
        Refunded
    }
}