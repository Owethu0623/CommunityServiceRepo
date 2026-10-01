using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class ApplicationDocument
    {
        [Key]
        public int ApplicationDocumentID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual TechnicianApplication Application { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Document Title")]
        public string DocumentTitle { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "File Path")]
        public string FilePath { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Content Type")]
        public string ContentType { get; set; }

        [Required]
        [Range(1, 10485760)]
        [Display(Name = "File Size")]
        public long FileSize { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; }

        [Required]
        [Display(Name = "Verification Status")]
        public ApplicationDocumentVerificationStatus VerificationStatus { get; set; }

        [StringLength(1000)]
        [Display(Name = "Verification Comments")]
        public string VerificationComments { get; set; }

        public int? VerifiedByAdministratorID { get; set; }

        [ForeignKey("VerifiedByAdministratorID")]
        public virtual Administrator VerifiedByAdministrator { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Verification Date")]
        public DateTime? VerificationDate { get; set; }
    }
}