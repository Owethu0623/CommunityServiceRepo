using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CommunityServiceProject.ViewModels
{
    public class ApplicationDocumentCreateViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        [Required(ErrorMessage = "Please select a document type.")]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; }

        [Required(ErrorMessage = "Please enter a document title.")]
        [StringLength(
            200,
            MinimumLength = 3,
            ErrorMessage = "The document title must be between 3 and 200 characters."
        )]
        [Display(Name = "Document Title")]
        public string DocumentTitle { get; set; }

        [Required(ErrorMessage = "Please select a document to upload.")]
        [Display(Name = "Document File")]
        public HttpPostedFileBase DocumentFile { get; set; }
    }
}