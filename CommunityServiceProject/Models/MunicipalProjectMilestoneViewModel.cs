using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    // =========================================================
    // US97 — MUNICIPAL PROJECT MILESTONES
    // =========================================================

    public class MunicipalProjectMilestonesViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public DateTime ProjectStartDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        public List<ProjectMilestoneItemViewModel> Milestones { get; set; }

        public int MilestoneCount { get; set; }

        public int CompletedMilestoneCount { get; set; }

        public int InProgressMilestoneCount { get; set; }

        public int DelayedMilestoneCount { get; set; }

        public MunicipalProjectMilestonesViewModel()
        {
            Milestones =
                new List<ProjectMilestoneItemViewModel>();
        }
    }


    public class ProjectMilestoneItemViewModel
    {
        public int ProjectMilestoneID { get; set; }

        [Required]
        [StringLength(
            200,
            MinimumLength = 3,
            ErrorMessage =
                "Milestone name must be between 3 and 200 characters."
        )]
        [Display(Name = "Milestone Name")]
        public string MilestoneName { get; set; }


        [StringLength(
            2000,
            ErrorMessage =
                "Description cannot exceed 2000 characters."
        )]
        [Display(Name = "Description")]
        public string Description { get; set; }


        [Required]
        [Display(Name = "Planned Start Date")]
        [DataType(DataType.Date)]
        public DateTime PlannedStartDate { get; set; }


        [Required]
        [Display(Name = "Planned Completion Date")]
        [DataType(DataType.Date)]
        public DateTime PlannedCompletionDate { get; set; }


        [Display(Name = "Actual Completion Date")]
        [DataType(DataType.Date)]
        public DateTime? ActualCompletionDate { get; set; }


        [Required]
        [Display(Name = "Status")]
        public ProjectMilestoneStatus Status { get; set; }


        [Required]
        [Range(
            0,
            100,
            ErrorMessage =
                "Progress must be between 0 and 100 percent."
        )]
        [Display(Name = "Progress")]
        public int ProgressPercentage { get; set; }


        [Required]
        [Display(Name = "Priority")]
        public MunicipalProjectPriority Priority { get; set; }


        [Required]
        [Range(
            1,
            9999,
            ErrorMessage =
                "Sequence number must be between 1 and 9999."
        )]
        [Display(Name = "Sequence")]
        public int SequenceNumber { get; set; }
    }
}