
using System;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianAccountDetailsViewModel
    {
        public int OnboardingID { get; set; }
        public int ApplicationID { get; set; }
        public int TechnicianID { get; set; }

        // Personal information carried from Citizen account
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string PersonalEmail { get; set; }

        public string ResidentialAddress { get; set; }

        // Technician login information
        public string MunicipalEmail { get; set; }

        public string TemporaryPassword { get; set; }

        public string AccountStatus { get; set; }

        public bool MustChangePassword { get; set; }
    }
}

