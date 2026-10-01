using System;
using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationReviewViewModel
    {
        public string SearchTerm { get; set; }

        public string StatusFilter { get; set; }

        public int? OpportunityFilter { get; set; }

        public int TotalApplications { get; set; }

        public int SubmittedApplications { get; set; }

        public int UnderReviewApplications { get; set; }

        public int ShortlistedApplications { get; set; }

        public int SelectedApplications { get; set; }

        public List<TechnicianApplicationReviewItemViewModel> Applications { get; set; }

        public List<TechnicianOpportunityFilterViewModel> Opportunities { get; set; }

        public TechnicianApplicationReviewViewModel()
        {
            Applications =
                new List<TechnicianApplicationReviewItemViewModel>();

            Opportunities =
                new List<TechnicianOpportunityFilterViewModel>();
        }
    }

    public class TechnicianApplicationReviewItemViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public int CitizenID { get; set; }

        public int? ScheduledAssessmentID { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        public TechnicianApplicationStatus Status { get; set; }

        public int DocumentCount { get; set; }

        public int PendingDocumentCount { get; set; }

        public bool HasDocuments { get; set; }

        public bool CanReview { get; set; }
    }

    public class TechnicianOpportunityFilterViewModel
    {
        public int OpportunityID { get; set; }

        public string OpportunityCode { get; set; }

        public string Title { get; set; }
    }
}