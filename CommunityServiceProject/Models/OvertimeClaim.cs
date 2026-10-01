using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class OvertimeClaim
    {
        [Key]
        public int OvertimeClaimID { get; set; }

        [Required]
        public int TechnicianID { get; set; }
        public int? PayrollID { get; set; }

        [Required]
        public DateTime OvertimeDate { get; set; }

        [Required]
        [Range(0.1, 24)]
        public decimal Hours { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }

        [Required]
        public OvertimeClaimStatus Status { get; set; }

        [Required]
        public DateTime DateSubmitted { get; set; }

        public DateTime? ReviewDate { get; set; }

        public int? ReviewedByAdministratorID { get; set; }

        [StringLength(500)]
        public string AdministratorComment { get; set; }

        public virtual Technician Technician { get; set; }

        public virtual Administrator ReviewedByAdministrator { get; set; }

        public virtual Payroll Payroll { get; set; }
    }
}