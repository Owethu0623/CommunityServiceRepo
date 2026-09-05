using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class ReassignmentRequestViewModel
    {
        public int AssignmentID { get; set; }

        public int RequestID { get; set; }

        public string RequestTitle { get; set; }

        [Required(ErrorMessage = "Please provide a reason for the reassignment request.")]
        [StringLength(1000, ErrorMessage = "The reason cannot exceed 1000 characters.")]
        [Display(Name = "Reason for Reassignment")]
        public string Reason { get; set; }
    }
}