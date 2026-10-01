using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CommunityServiceProject.Models
{
    public class TechnicianApplicationScreening
    {
        [Key]
        public int ScreeningID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual TechnicianApplication Application { get; set; }

        [Required]
        [Display(Name = "Qualification Assessment")]
        public ApplicantScreeningResult QualificationAssessment { get; set; }

        [Required]
        [Display(Name = "Experience Assessment")]
        public ApplicantScreeningResult ExperienceAssessment { get; set; }

        [Required]
        [Display(Name = "Requirements Assessment")]
        public ApplicantScreeningResult RequirementsAssessment { get; set; }

        [Required]
        [Display(Name = "Overall Screening Result")]
        public ApplicantScreeningResult OverallResult { get; set; }

        [Required]
        [StringLength(
            3000,
            MinimumLength = 20,
            ErrorMessage = "Screening comments must be between 20 and 3000 characters."
        )]
        [Display(Name = "Screening Comments")]
        public string ScreeningComments { get; set; }

        [Required]
        public int ScreenedByAdministratorID { get; set; }

        [ForeignKey("ScreenedByAdministratorID")]
        public virtual Administrator ScreenedByAdministrator { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ScreeningDate { get; set; }
    }
}