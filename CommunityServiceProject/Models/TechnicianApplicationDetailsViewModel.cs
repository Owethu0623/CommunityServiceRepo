
using CommunityServiceProject.Models;
using System;
using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationDetailsViewModel
    {
        public int ApplicationID { get; set; }

        public int OpportunityID { get; set; }

        public string ApplicationReference { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public string EmploymentType { get; set; }

        public bool IsEditable { get; set; }

        public int? NumberOfPositions { get; set; }

        public bool HasDocuments { get; set; }

        public int DocumentCount { get; set; }

        public bool CanSubmit { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        public TechnicianApplicationStatus Status { get; set; }

        public string CoverLetter { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public bool IsSuccessful { get; set; }

        public bool IsUnsuccessful { get; set; }

        public List<TechnicianOnboardingStatusViewModel> TechnicianOnboardings
        {
            get;
            set;
        }

        public TechnicianApplicationDetailsViewModel()
        {
            TechnicianOnboardings =
                new List<TechnicianOnboardingStatusViewModel>();
        }
    }

    public class TechnicianOnboardingStatusViewModel
    {
        public int OnboardingID { get; set; }

        public int? TechnicianID { get; set; }

        public string TechnicianName { get; set; }

        public string MunicipalEmail { get; set; }

        public string AccountStatus { get; set; }

        public DateTime OnboardingDate { get; set; }
    }
}
