using CommunityServiceProject.Models;
using System;
using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianOpportunityListViewModel
    {
        public string SearchTerm { get; set; }

        public string EmploymentType { get; set; }

        public string ClosingFilter { get; set; }

        public int TotalOpportunities { get; set; }

        public List<TechnicianOpportunityListItemViewModel> Opportunities { get; set; }

        public TechnicianOpportunityListViewModel()
        {
            Opportunities =
                new List<TechnicianOpportunityListItemViewModel>();
        }
    }

    public class TechnicianOpportunityListItemViewModel
    {
        public int OpportunityID { get; set; }

        public string OpportunityCode { get; set; }

        public string Title { get; set; }

        public string EmploymentType { get; set; }

        public int? NumberOfPositions { get; set; }

        public DateTime ApplicationStartDate { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        public string Requirements { get; set; }

        public TechnicianOpportunityStatus Status { get; set; }

        public bool IsOpenForApplications { get; set; }

        public int DaysRemaining { get; set; }
    }
}