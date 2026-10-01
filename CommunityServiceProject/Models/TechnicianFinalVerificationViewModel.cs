using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationFinalVerificationViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public DateTime ApplicationDate { get; set; }

        public TechnicianApplicationStatus ApplicationStatus { get; set; }

        public string ScreeningResult { get; set; }

        public string ScreeningComments { get; set; }

        public int TotalDocuments { get; set; }

        public int AcceptedDocuments { get; set; }

        public int RejectedDocuments { get; set; }

        public string AssessmentType { get; set; }

        public DateTime? AssessmentDate { get; set; }

        public string AssessmentResult { get; set; }

        public decimal? AssessmentScore { get; set; }

        public string AssessmentComments { get; set; }

        public string InterviewMethod { get; set; }

        public DateTime? InterviewDate { get; set; }

        public string InterviewLocation { get; set; }

        public string InterviewOutcome { get; set; }

        public string InterviewComments { get; set; }

        public string SelectionComments { get; set; }

        public DateTime? SelectionDate { get; set; }

        [Required(ErrorMessage =
            "Please select the final verification result.")]
        [Display(Name = "Final Verification Result")]
        public ApplicationFinalVerificationResult? Result { get; set; }

        [Required(
            ErrorMessage =
                "Please enter the final verification comments.")]
        [StringLength(
            3000,
            MinimumLength = 3,
            ErrorMessage =
                "Verification comments must be between 3 and 3000 characters.")]
        [Display(Name = "Verification Comments")]
        public string Comments { get; set; }

        [Required(ErrorMessage =
            "Please confirm that you have completed the final verification.")]
        [Display(Name = "Verification Confirmation")]
        public bool ConfirmVerification { get; set; }
    }
}