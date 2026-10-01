using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationAssessmentResultViewModel
    {
        public int AssessmentID { get; set; }

        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public string AssessmentType { get; set; }

        public System.DateTime AssessmentDate { get; set; }

        public string Location { get; set; }

        public string Instructions { get; set; }

        public ApplicationAssessmentStatus AssessmentStatus { get; set; }

        [Required(ErrorMessage = "Please enter the assessment result.")]
        [StringLength(
            2000,
            MinimumLength = 3,
            ErrorMessage = "The assessment result must be between 3 and 2000 characters."
        )]
        [Display(Name = "Assessment Result")]
        public string Result { get; set; }

        [Range(
            0,
            100,
            ErrorMessage = "The assessment score must be between 0 and 100."
        )]
        [Display(Name = "Score")]
        public decimal? Score { get; set; }

        [StringLength(
            3000,
            ErrorMessage = "Comments cannot exceed 3000 characters."
        )]
        [Display(Name = "Assessment Comments")]
        public string Comments { get; set; }

        public bool CanRecordResult { get; set; }
    }
}