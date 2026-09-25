using System;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianOpportunityDetailsViewModel
    {
        public int OpportunityID { get; set; }

        public string OpportunityCode { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Responsibilities { get; set; }

        public string Requirements { get; set; }

        public string RequiredQualifications { get; set; }

        public string RequiredExperience { get; set; }

        public string ApplicationInstructions { get; set; }

        public string EmploymentType { get; set; }

        public int? NumberOfPositions { get; set; }

        public DateTime ApplicationStartDate { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        public DateTime? PublishedDate { get; set; }

        public TechnicianOpportunityStatus Status { get; set; }

        public bool IsOpenForApplications { get; set; }

        public int DaysRemaining { get; set; }
    }
}