using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationDocumentVerificationResultViewModel
    {
        public int ApplicationDocumentID { get; set; }

        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public string DocumentType { get; set; }

        public string DocumentTitle { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long FileSize { get; set; }

        public System.DateTime DateSubmitted { get; set; }

        [Required(ErrorMessage = "Please select a verification result.")]
        [Display(Name = "Verification Result")]
        public ApplicationDocumentVerificationStatus VerificationStatus { get; set; }

        [StringLength(
            1000,
            ErrorMessage = "Verification comments cannot exceed 1000 characters.")]
        [Display(Name = "Verification Comments")]
        public string VerificationComments { get; set; }
    }
}