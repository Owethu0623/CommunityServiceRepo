using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationCreateViewModel
    {
        public int OpportunityID { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public string EmploymentType { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        public int? NumberOfPositions { get; set; }

        [Required(
            ErrorMessage =
                "Please provide a cover letter.")]
        [StringLength(
            4000,
            MinimumLength = 50,
            ErrorMessage =
                "The cover letter must be between 50 and 4000 characters.")]
        [Display(Name = "Cover Letter")]
        public string CoverLetter { get; set; }
    }
}