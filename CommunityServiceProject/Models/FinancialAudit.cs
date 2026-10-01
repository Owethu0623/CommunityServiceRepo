using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class FinancialAudit
    {
        [Key]
        public int FinancialAuditID { get; set; }

        [Required]
        public int FinanceOfficerID { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; }

        [Required]
        public int EntityID { get; set; }

        [Required]
        public FinancialAuditAction Action { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public virtual FinanceOfficer FinanceOfficer { get; set; }
    }
}