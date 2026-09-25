using System;
using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectProgressViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        public int CurrentProgressPercentage { get; set; }

        public DateTime? LatestProgressDate { get; set; }

        public string LatestProgressSummary { get; set; }

        public string LatestCurrentActivity { get; set; }

        public string LatestNextPlannedActivity { get; set; }

        public string LatestRecordedBy { get; set; }

        public ProjectProgressEntryViewModel Entry { get; set; }

        public List<ProjectProgressItemViewModel> ProgressRecords { get; set; }

        public MunicipalProjectProgressViewModel()
        {
            Entry = new ProjectProgressEntryViewModel();

            ProgressRecords =
                new List<ProjectProgressItemViewModel>();
        }
    }

    public class ProjectProgressItemViewModel
    {
        public int ProjectProgressID { get; set; }

        public int ProgressPercentage { get; set; }

        public string ProgressSummary { get; set; }

        public string CurrentActivity { get; set; }

        public string IssuesEncountered { get; set; }

        public string NextPlannedActivity { get; set; }

        public DateTime ProgressDate { get; set; }

        public DateTime DateRecorded { get; set; }

        public string RecordedByAdministratorName { get; set; }
    }
}