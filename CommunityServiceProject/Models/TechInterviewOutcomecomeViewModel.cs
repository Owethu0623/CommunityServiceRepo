using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationInterviewOutcomeViewModel
    {
        public int InterviewID { get; set; }

        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public DateTime ApplicationDate { get; set; }

        public string InterviewMethod { get; set; }

        public DateTime InterviewDate { get; set; }

        public string Location { get; set; }

        public ApplicationInterviewStatus InterviewStatus { get; set; }

        [Required(ErrorMessage = "Please enter the interview outcome.")]
        [StringLength(
            2000,
            MinimumLength = 3,
            ErrorMessage = "The interview outcome must be between 3 and 2000 characters."
        )]
        [Display(Name = "Interview Outcome")]
        public string Outcome { get; set; }

        [StringLength(
            3000,
            ErrorMessage = "Interview comments cannot exceed 3000 characters."
        )]
        [Display(Name = "Interview Comments")]
        public string Comments { get; set; }

        public bool CanRecordOutcome { get; set; }
    }
}