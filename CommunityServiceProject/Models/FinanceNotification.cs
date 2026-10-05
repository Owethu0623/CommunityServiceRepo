using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class FinanceNotification
    {
        [Key]
        public int FinanceNotificationID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [ForeignKey("CitizenID")]
        public virtual Citizen Citizen { get; set; }

        public int? InvoiceID { get; set; }

        [ForeignKey("InvoiceID")]
        public virtual Invoice Invoice { get; set; }

        public int? PaymentID { get; set; }

        [ForeignKey("PaymentID")]
        public virtual Payment Payment { get; set; }

        public int? RefundID { get; set; }

        [ForeignKey("RefundID")]
        public virtual Refund Refund { get; set; }

        public int? ReceiptID { get; set; }

        [ForeignKey("ReceiptID")]
        public virtual Receipt Receipt { get; set; }

        [Required]
        public FinanceNotificationType NotificationType { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }
    }
}