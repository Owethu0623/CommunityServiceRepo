using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class ProjectMilestone
    {
        [Key]
        public int ProjectMilestoneID { get; set; }


        [Required]
        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public virtual MunicipalProject Project { get; set; }


        [Required]
        [StringLength(200, MinimumLength = 3)]
        [Display(Name = "Milestone Name")]
        public string MilestoneName { get; set; }


        [StringLength(2000)]
        [Display(Name = "Description")]
        public string Description { get; set; }


        [Required]
        [Display(Name = "Planned Start Date")]
        public DateTime PlannedStartDate { get; set; }


        [Required]
        [Display(Name = "Planned Completion Date")]
        public DateTime PlannedCompletionDate { get; set; }


        [Display(Name = "Actual Completion Date")]
        public DateTime? ActualCompletionDate { get; set; }


        [Required]
        [Display(Name = "Status")]
        public ProjectMilestoneStatus Status { get; set; }


        [Required]
        [Range(0, 100)]
        [Display(Name = "Progress")]
        public int ProgressPercentage { get; set; }


        [Required]
        [Display(Name = "Priority")]
        public MunicipalProjectPriority Priority { get; set; }


        [Required]
        [Range(1, 9999)]
        [Display(Name = "Sequence")]
        public int SequenceNumber { get; set; }


        [Required]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }


        [Required]
        [Display(Name = "Created By")]
        public int CreatedByAdministratorID { get; set; }

        [ForeignKey("CreatedByAdministratorID")]
        public virtual Administrator CreatedByAdministrator { get; set; }


        [Display(Name = "Last Updated")]
        public DateTime? LastUpdatedDate { get; set; }


        [Display(Name = "Last Updated By")]
        public int? LastUpdatedByAdministratorID { get; set; }

        [ForeignKey("LastUpdatedByAdministratorID")]
        public virtual Administrator LastUpdatedByAdministrator { get; set; }
    }


    public enum ProjectMilestoneStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Delayed,
        Cancelled
    }
}