using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationSelectViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public DateTime ApplicationDate { get; set; }

        public TechnicianApplicationStatus ApplicationStatus { get; set; }

        public string InterviewMethod { get; set; }

        public DateTime InterviewDate { get; set; }

        public string InterviewLocation { get; set; }

        public string InterviewOutcome { get; set; }

        public string InterviewComments { get; set; }

        [StringLength(
            3000,
            ErrorMessage = "Selection comments cannot exceed 3000 characters.")]
        [Display(Name = "Selection Comments")]
        public string Comments { get; set; }

        [Required(ErrorMessage =
            "Please confirm that you want to select this applicant.")]
        [Display(Name = "Selection Confirmation")]
        public bool ConfirmSelection { get; set; }
    }
}