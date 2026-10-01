using System;
using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationDocumentVerificationViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public TechnicianApplicationStatus ApplicationStatus { get; set; }

        public int TotalDocuments { get; set; }

        public int PendingDocuments { get; set; }

        public int AcceptedDocuments { get; set; }

        public int RejectedDocuments { get; set; }

        public List<TechnicianApplicationDocumentVerificationItemViewModel> Documents { get; set; }

        public TechnicianApplicationDocumentVerificationViewModel()
        {
            Documents =
                new List<TechnicianApplicationDocumentVerificationItemViewModel>();
        }
    }

    public class TechnicianApplicationDocumentVerificationItemViewModel
    {
        public int ApplicationDocumentID { get; set; }

        public string DocumentType { get; set; }

        public string DocumentTitle { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime DateSubmitted { get; set; }

        public ApplicationDocumentVerificationStatus VerificationStatus { get; set; }

        public string VerificationComments { get; set; }

        public DateTime? VerificationDate { get; set; }

        public string VerifiedByAdministratorName { get; set; }

        public bool CanRecordVerification { get; set; }
    }
}