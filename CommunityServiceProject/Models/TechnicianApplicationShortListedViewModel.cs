using System;
using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationShortlistedViewModel
    {
        public string SearchTerm { get; set; }

        public int? OpportunityFilter { get; set; }

        public int TotalShortlisted { get; set; }

        public List<TechnicianApplicationShortlistedItemViewModel> Applicants { get; set; }

        public List<TechnicianOpportunityFilterViewModel> Opportunities { get; set; }

        public TechnicianApplicationShortlistedViewModel()
        {
            Applicants =
                new List<TechnicianApplicationShortlistedItemViewModel>();

            Opportunities =
                new List<TechnicianOpportunityFilterViewModel>();
        }
    }

    public class TechnicianApplicationShortlistedItemViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public int CitizenID { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public TechnicianApplicationStatus Status { get; set; }

        public bool CanScheduleAssessment { get; set; }
    }
}