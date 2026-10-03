using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class FinancialAuditRecord
    {
        [Key]
        public int FinancialAuditRecordID { get; set; }

        [Required]
        public DateTime AuditDate { get; set; }

        [Required]
        public int FinanceOfficerID { get; set; }

        [Required]
        [StringLength(100)]
        public string PerformedBy { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; }

        [Required]
        public int EntityID { get; set; }

        [StringLength(100)]
        public string Reference { get; set; }

        [StringLength(50)]
        public string PreviousStatus { get; set; }

        [StringLength(50)]
        public string NewStatus { get; set; }

        [Column(TypeName = "decimal")]
        public decimal? Amount { get; set; }

        [StringLength(1000)]
        public string Details { get; set; }

        [StringLength(100)]
        public string IPAddress { get; set; }
    }
}