using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Receipt
    {
        [Key]
        public int ReceiptID { get; set; }

        [Required]
        [StringLength(40)]
        public string ReceiptNumber { get; set; }
        [Required]
        [Index("IX_Receipt_PaymentID", IsUnique = true)]
        public int PaymentID { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public virtual Payment Payment { get; set; }
    }
}