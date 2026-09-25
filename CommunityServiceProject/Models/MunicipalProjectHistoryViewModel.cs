using System;
using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectHistoryViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public string CurrentStatus { get; set; }

        public string Priority { get; set; }

        public int TotalHistoryRecords { get; set; }

        public List<ProjectHistoryItemViewModel> HistoryRecords { get; set; }

        public MunicipalProjectHistoryViewModel()
        {
            HistoryRecords =
                new List<ProjectHistoryItemViewModel>();
        }
    }


    public class ProjectHistoryItemViewModel
    {
        public int ProjectHistoryID { get; set; }

        public string ActionType { get; set; }

        public string Description { get; set; }

        public string PreviousStatus { get; set; }

        public string NewStatus { get; set; }

        public DateTime ActionDate { get; set; }

        public string PerformedByAdministratorName { get; set; }
    }
}