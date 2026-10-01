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

        [Required]
        public int TechnicianID { get; set; }

        [ForeignKey("TechnicianID")]
        public virtual Technician Technician { get; set; }

        [Required]
        public int OnboardedByAdministratorID { get; set; }

        [ForeignKey("OnboardedByAdministratorID")]
        public virtual Administrator OnboardedByAdministrator { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime OnboardingDate { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Municipal Email")]
        public string MunicipalEmail { get; set; }
    }
}