using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectIndexViewModel
    {
        public List<MunicipalProjectIndexItemViewModel> Projects { get; set; }

        public string SearchTerm { get; set; }
        public string SelectedStatus { get; set; }
        public string SelectedPriority { get; set; }
        public string SelectedWard { get; set; }
        public string SelectedProjectType { get; set; }

        public int TotalProjects { get; set; }
        public int PlannedProjects { get; set; }
        public int ApprovedProjects { get; set; }
        public int InProgressProjects { get; set; }
        public int OnHoldProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int CancelledProjects { get; set; }

        public List<string> StatusOptions { get; set; }
        public List<string> PriorityOptions { get; set; }
        public List<string> WardOptions { get; set; }
        public List<string> ProjectTypeOptions { get; set; }

        public MunicipalProjectIndexViewModel()
        {
            Projects = new List<MunicipalProjectIndexItemViewModel>();
            StatusOptions = new List<string>();
            PriorityOptions = new List<string>();
            WardOptions = new List<string>();
            ProjectTypeOptions = new List<string>();
        }
    }


    public class MunicipalProjectIndexItemViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string WardName { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public decimal? EstimatedBudget { get; set; }

        public System.DateTime StartDate { get; set; }

        public System.DateTime? ExpectedCompletionDate { get; set; }

        public System.DateTime DateRegistered { get; set; }

        public string ResponsibleAdministratorName { get; set; }
    }
}