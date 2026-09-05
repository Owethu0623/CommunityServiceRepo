using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class AssignmentIssueViewModel
    {
        public int AssignmentID { get; set; }

        public int RequestID { get; set; }

        public string RequestTitle { get; set; }

        [Required(ErrorMessage = "Please select an issue type.")]
        [Display(Name = "Issue Type")]
        public AssignmentIssueType IssueType { get; set; }

        [Required(ErrorMessage = "Please provide a reason for the issue.")]
        [StringLength(1000, ErrorMessage = "The reason cannot exceed 1000 characters.")]
        [Display(Name = "Reason")]
        public string Reason { get; set; }
    }
}