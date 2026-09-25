using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class ProjectProgress
    {
        [Key]
        public int ProjectProgressID { get; set; }


        [Required]
        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public virtual MunicipalProject Project { get; set; }


        [Required]
        [Range(0, 100)]
        [Display(Name = "Progress Percentage")]
        public int ProgressPercentage { get; set; }


        [Required]
        [StringLength(4000, MinimumLength = 10)]
        [Display(Name = "Progress Summary")]
        public string ProgressSummary { get; set; }


        [StringLength(1000)]
        [Display(Name = "Current Activity")]
        public string CurrentActivity { get; set; }


        [StringLength(2000)]
        [Display(Name = "Issues Encountered")]
        public string IssuesEncountered { get; set; }


        [StringLength(2000)]
        [Display(Name = "Next Planned Activity")]
        public string NextPlannedActivity { get; set; }


        [Required]
        [Display(Name = "Progress Date")]
        public DateTime ProgressDate { get; set; }


        [Required]
        [Display(Name = "Date Recorded")]
        public DateTime DateRecorded { get; set; }


        [Required]
        [Display(Name = "Recorded By")]
        public int RecordedByAdministratorID { get; set; }

        [ForeignKey("RecordedByAdministratorID")]
        public virtual Administrator RecordedByAdministrator { get; set; }
    }
}