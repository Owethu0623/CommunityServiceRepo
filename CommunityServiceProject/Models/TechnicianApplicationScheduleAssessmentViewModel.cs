using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationScheduleAssessmentViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public DateTime ApplicationDate { get; set; }

        public TechnicianApplicationStatus ApplicationStatus { get; set; }

        [Required(ErrorMessage = "Please select an assessment type.")]
        [StringLength(100)]
        [Display(Name = "Assessment Type")]
        public string AssessmentType { get; set; }

        [Required(ErrorMessage = "Please select the assessment date and time.")]
        [Display(Name = "Assessment Date and Time")]
        public DateTime AssessmentDate { get; set; }

        [Required(ErrorMessage = "Please enter the assessment location.")]
        [StringLength(
            300,
            MinimumLength = 3,
            ErrorMessage = "The assessment location must be between 3 and 300 characters."
        )]
        [Display(Name = "Assessment Location")]
        public string Location { get; set; }

        [StringLength(
            3000,
            ErrorMessage = "Instructions cannot exceed 3000 characters."
        )]
        [Display(Name = "Instructions")]
        public string Instructions { get; set; }
    }
}