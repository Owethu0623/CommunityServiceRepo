using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class PayrollPayment
    {
        [Key]
        public int PayrollPaymentID { get; set; }

        [Required]
        [Index("IX_PayrollPayment_PayrollID", IsUnique = true)]
        public int PayrollID { get; set; }

        [Required]
        [StringLength(40)]
        public string TransactionReference { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Amount { get; set; }

        [Required]
        public PayrollPaymentStatus Status { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        public DateTime? ProcessedDate { get; set; }

        [StringLength(500)]
        public string FailureReason { get; set; }

        public virtual Payroll Payroll { get; set; }
    }
}