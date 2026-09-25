using CommunityServiceProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public int? NumberOfPositions { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        public TechnicianApplicationStatus Status { get; set; }

        public string CoverLetter { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public bool IsSuccessful { get; set; }

        public bool IsUnsuccessful { get; set; }
    }
}