using System;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianOnboardingSuccessViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public TechnicianApplicationStatus ApplicationStatus { get; set; }

        public int TechnicianID { get; set; }

        public string TechnicianName { get; set; }

        public string MunicipalEmail { get; set; }

        public string PhoneNumber { get; set; }

        public AccountStatus AccountStatus { get; set; }

        public int CitizenID { get; set; }
        public int SkillID { get; set; }

        public string SkillName { get; set; }

        public string CitizenName { get; set; }

        public DateTime OnboardingDate { get; set; }

        public string AdministratorName { get; set; }
    }
}