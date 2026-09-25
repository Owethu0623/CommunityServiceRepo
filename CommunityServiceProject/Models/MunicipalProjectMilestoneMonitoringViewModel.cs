using System;
using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectMilestoneMonitoringViewModel
    {
        public int ProjectID { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string ProjectType { get; set; }
        public string ProjectLocation { get; set; }
        public string WardName { get; set; }
        public string ProjectStatus { get; set; }
        public string ProjectPriority { get; set; }

        public int TotalMilestones { get; set; }
        public int CompletedMilestones { get; set; }
        public int InProgressMilestones { get; set; }
        public int NotStartedMilestones { get; set; }
        public int DelayedMilestones { get; set; }
        public int OverdueMilestones { get; set; }
        public int IncompleteMilestones { get; set; }

        public decimal OverallProgress { get; set; }

        public List<ProjectMilestoneMonitoringItemViewModel> Milestones { get; set; }

        public MunicipalProjectMilestoneMonitoringViewModel()
        {
            Milestones =
                new List<ProjectMilestoneMonitoringItemViewModel>();
        }
    }


    public class ProjectMilestoneMonitoringItemViewModel
    {
        public int ProjectMilestoneID { get; set; }

        public string MilestoneName { get; set; }

        public string Description { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime PlannedStartDate { get; set; }

        public DateTime PlannedCompletionDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        public string Status { get; set; }

        public int ProgressPercentage { get; set; }

        public string Priority { get; set; }

        public bool IsOverdue { get; set; }

        public bool IsIncomplete { get; set; }

        public string MonitoringStatus { get; set; }

        public int DaysRemaining { get; set; }

        public int DaysOverdue { get; set; }
    }
}