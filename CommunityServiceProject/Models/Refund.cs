using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Refund
    {
        [Key]
        public int RefundID { get; set; }

        [Required]
        [StringLength(40)]
        public string RefundReference { get; set; }

        [Required]
        public int PaymentID { get; set; }

        [Required]
        public int InvoiceID { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Amount { get; set; }

        [Required]
        public RefundStatus Status { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        public DateTime? ProcessedDate { get; set; }

        public int? ProcessedByFinanceOfficerID { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        public virtual Payment Payment { get; set; }

        public virtual Invoice Invoice { get; set; }

        public virtual FinanceOfficer ProcessedByFinanceOfficer { get; set; }
    }
}


