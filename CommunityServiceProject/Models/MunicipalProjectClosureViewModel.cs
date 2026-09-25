using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectClosureViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public string CurrentStatus { get; set; }

        public string Priority { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        [Required(ErrorMessage = "Please provide a closure summary.")]
        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "Closure summary must be between 10 and 2000 characters."
        )]
        [Display(Name = "Closure Summary")]
        public string ClosureSummary { get; set; }
    }
}