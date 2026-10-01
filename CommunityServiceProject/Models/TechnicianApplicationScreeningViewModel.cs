using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationScreeningViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public string RequiredQualifications { get; set; }

        public string RequiredExperience { get; set; }

        public string Requirements { get; set; }

        public string CoverLetter { get; set; }

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

        [Required(ErrorMessage = "Please provide screening comments.")]
        [StringLength(
            3000,
            MinimumLength = 20,
            ErrorMessage = "Screening comments must be between 20 and 3000 characters."
        )]
        [Display(Name = "Screening Comments")]
        public string ScreeningComments { get; set; }
    }
}