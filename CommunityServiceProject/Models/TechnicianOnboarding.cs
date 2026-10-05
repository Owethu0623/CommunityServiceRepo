using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class TechnicianOnboarding
    {
        [Key]
        public int OnboardingID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual TechnicianApplication Application { get; set; }

        // Technician may not exist yet when HR completes onboarding; make nullable.
        public int? TechnicianID { get; set; }

        [ForeignKey("TechnicianID")]
        public virtual Technician Technician { get; set; }

        // Administrator account creation happens after HR onboarding; keep
        // Administrator audit but allow it to be nullable until admin completes account creation.
        public int? OnboardedByAdministratorID { get; set; }

        [ForeignKey("OnboardedByAdministratorID")]
        public virtual Administrator OnboardedByAdministrator { get; set; }

        // To support HR onboarding and Administrator account creation handover
        // keep the Administrator audit field but allow an optional HR officer
        // record if HR performed the onboarding step.
        public int? OnboardedByHROfficerID { get; set; }

        [ForeignKey("OnboardedByHROfficerID")]
        public virtual HROfficer OnboardedByHROfficer { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime OnboardingDate { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Municipal Email")]
        public string MunicipalEmail { get; set; }
    }
}