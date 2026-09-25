using System;
using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationStatusViewModel
    {
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }

        public int TotalApplications { get; set; }
        public int ActiveApplications { get; set; }
        public int SelectedApplications { get; set; }
        public int UnsuccessfulApplications { get; set; }

        public List<TechnicianApplicationStatusItemViewModel> Applications { get; set; }

        public TechnicianApplicationStatusViewModel()
        {
            Applications = new List<TechnicianApplicationStatusItemViewModel>();
        }
    }

    public class TechnicianApplicationStatusItemViewModel
    {
        public int ApplicationID { get; set; }
        public string ApplicationReference { get; set; }

        public int OpportunityID { get; set; }
        public string OpportunityCode { get; set; }
        public string OpportunityTitle { get; set; }

        public string EmploymentType { get; set; }

        public DateTime ApplicationDate { get; set; }
        public DateTime ApplicationDeadline { get; set; }

        public TechnicianApplicationStatus Status { get; set; }

        public bool IsActive { get; set; }
        public bool IsSuccessful { get; set; }
        public bool IsUnsuccessful { get; set; }
    }
}

   