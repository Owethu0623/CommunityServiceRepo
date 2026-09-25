using System;
using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianOpportunityManagementViewModel
    {
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }

        public int TotalOpportunities { get; set; }
        public int DraftCount { get; set; }
        public int PublishedCount { get; set; }
        public int ClosedCount { get; set; }
        public int CancelledCount { get; set; }

        public List<TechnicianOpportunityManagementItemViewModel> Opportunities
        {
            get;
            set;
        }

        public TechnicianOpportunityManagementViewModel()
        {
            Opportunities =
                new List<TechnicianOpportunityManagementItemViewModel>();
        }
    }

    public class TechnicianOpportunityManagementItemViewModel
    {
        public int OpportunityID { get; set; }
        public string OpportunityCode { get; set; }
        public string Title { get; set; }
        public string EmploymentType { get; set; }
        public int? NumberOfPositions { get; set; }

        public DateTime ApplicationStartDate { get; set; }
        public DateTime ApplicationDeadline { get; set; }

        public TechnicianOpportunityStatus Status { get; set; }

        public bool CanEdit { get; set; }
        public bool CanPublish { get; set; }
        public bool CanClose { get; set; }
        public bool CanCancel { get; set; }

        public int? DaysUntilDeadline { get; set; }
    }
}