using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class ProjectObjective
    {
        [Key]
        public int ProjectObjectiveID { get; set; }

        [Required]
        [Display(Name = "Project")]
        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public virtual MunicipalProject Project { get; set; }


        [Required]
        [StringLength(200, MinimumLength = 3)]
        [Display(Name = "Objective Title")]
        public string ObjectiveTitle { get; set; }


        [Required]
        [StringLength(2000, MinimumLength = 10)]
        [Display(Name = "Objective Description")]
        public string ObjectiveDescription { get; set; }


        [Required]
        [Display(Name = "Priority")]
        public MunicipalProjectPriority Priority { get; set; }


        [Display(Name = "Target Date")]
        public DateTime? TargetDate { get; set; }


        [Required]
        [Display(Name = "Status")]
        public ProjectObjectiveStatus Status { get; set; }


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


    public enum ProjectObjectiveStatus
    {
        NotStarted,
        InProgress,
        Achieved,
        NotAchieved,
        Cancelled
    }
}