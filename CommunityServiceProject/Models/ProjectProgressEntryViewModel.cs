using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class ProjectProgressEntryViewModel
    {
        public int ProjectID { get; set; }

        [Required(ErrorMessage = "Please enter the project progress percentage.")]
        [Range(0, 100,
            ErrorMessage = "Progress percentage must be between 0 and 100.")]
        [Display(Name = "Progress Percentage")]
        public int ProgressPercentage { get; set; }

        [Required(ErrorMessage = "Please provide a progress summary.")]
        [StringLength(4000, MinimumLength = 10,
            ErrorMessage = "Progress summary must be between 10 and 4000 characters.")]
        [Display(Name = "Progress Summary")]
        public string ProgressSummary { get; set; }

        [StringLength(1000,
            ErrorMessage = "Current activity cannot exceed 1000 characters.")]
        [Display(Name = "Current Activity")]
        public string CurrentActivity { get; set; }

        [StringLength(2000,
            ErrorMessage = "Issues encountered cannot exceed 2000 characters.")]
        [Display(Name = "Issues Encountered")]
        public string IssuesEncountered { get; set; }

        [StringLength(2000,
            ErrorMessage = "Next planned activity cannot exceed 2000 characters.")]
        [Display(Name = "Next Planned Activity")]
        public string NextPlannedActivity { get; set; }

        [Required(ErrorMessage = "Please select the progress date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Progress Date")]
        public DateTime? ProgressDate { get; set; }
    }
}