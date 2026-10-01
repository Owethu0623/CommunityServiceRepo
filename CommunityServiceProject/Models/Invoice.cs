using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CommunityServiceProject.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceID { get; set; }

        [Required]
        [StringLength(30)]
        [Index(IsUnique = true)]
        public string InvoiceNumber { get; set; }

        [Required]
        public int MunicipalServiceRequestID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [Required]
        public int FeeScheduleID { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal AmountPaid { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Balance { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? PaidDate { get; set; }

        public virtual MunicipalServiceRequest MunicipalServiceRequest { get; set; }

        public virtual Citizen Citizen { get; set; }

        public virtual FeeSchedule FeeSchedule { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Refund> Refunds { get; set; }


        public Invoice()
        {
            Payments = new HashSet<Payment>();
            Refunds = new HashSet<Refund>();
        }
    }
}