using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectCompletionViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public string CurrentStatus { get; set; }

        public string Priority { get; set; }

        public int TotalMilestones { get; set; }

        public int CompletedMilestones { get; set; }

        public int IncompleteMilestones { get; set; }

        public decimal OverallProgress { get; set; }

        [Required(ErrorMessage = "Please provide a completion summary.")]
        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "Completion summary must be between 10 and 2000 characters."
        )]
        [Display(Name = "Completion Summary")]
        public string CompletionSummary { get; set; }

        [Required(ErrorMessage = "Please enter the actual completion date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Actual Completion Date")]
        public DateTime ActualCompletionDate { get; set; }
    }
}