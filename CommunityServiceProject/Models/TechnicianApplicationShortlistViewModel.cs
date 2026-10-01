using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationShortlistViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public DateTime ApplicationDate { get; set; }

        public TechnicianApplicationStatus ApplicationStatus { get; set; }

        public ApplicantScreeningResult ScreeningResult { get; set; }

        public int TotalDocuments { get; set; }

        public int AcceptedDocuments { get; set; }

        public int RejectedDocuments { get; set; }

        public int PendingDocuments { get; set; }

        public bool ScreeningPassed { get; set; }

        public bool DocumentsVerified { get; set; }

        public bool EligibleForShortlisting { get; set; }

        [Required(ErrorMessage = "Please confirm the shortlisting decision.")]
        public bool ConfirmShortlisting { get; set; }
    }
}