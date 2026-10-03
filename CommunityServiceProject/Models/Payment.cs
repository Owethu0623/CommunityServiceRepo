using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        [StringLength(40)]
        [Index(IsUnique = true)]
        public string TransactionReference { get; set; }

        [Required]
        public int InvoiceID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        public DateTime? ProcessedDate { get; set; }
        [StringLength(500)]
        public string ProofOfPaymentPath { get; set; }

        [StringLength(100)]
        public string PaymentMethod { get; set; }

        [StringLength(500)]
        public string FailureReason { get; set; }

        public virtual Invoice Invoice { get; set; }

        public virtual Citizen Citizen { get; set; }

        public virtual ICollection<Receipt> Receipts { get; set; }

        public Payment()
        {
            Receipts = new HashSet<Receipt>();
        }
    }
}