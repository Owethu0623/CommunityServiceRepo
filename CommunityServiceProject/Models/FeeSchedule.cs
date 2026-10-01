using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class FeeSchedule
    {
        [Key]
        public int FeeScheduleID { get; set; }

        [Required]
        public int ServiceTypeID { get; set; }

        [Required]
        [StringLength(50)]
        public string FeeType { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public int CreatedByFinanceOfficerID { get; set; }

        public virtual ServiceType ServiceType { get; set; }

        public virtual FinanceOfficer CreatedByFinanceOfficer { get; set; }
    }
}