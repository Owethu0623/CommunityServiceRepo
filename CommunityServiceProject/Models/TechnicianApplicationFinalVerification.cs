using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class TechnicianApplicationFinalVerification
    {
        [Key]
        public int FinalVerificationID { get; set; }

        [Index(
            "IX_TechnicianApplicationFinalVerification_Application",
            IsUnique = true)]
        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual TechnicianApplication Application { get; set; }

        [Required]
        [Display(Name = "Verification Result")]
        public ApplicationFinalVerificationResult Result { get; set; }

        [Required]
        [StringLength(
            3000,
            MinimumLength = 3,
            ErrorMessage =
                "Verification comments must be between 3 and 3000 characters.")]
        [Display(Name = "Verification Comments")]
        public string Comments { get; set; }

        // Administrator verification is optional because HR now owns recruitment.
        public int? VerifiedByAdministratorID { get; set; }

        [ForeignKey("VerifiedByAdministratorID")]
        public virtual Administrator VerifiedByAdministrator { get; set; }

        // HR verification is optional because older/existing records
        // may have been verified by an Administrator.
        public int? VerifiedByHROfficerID { get; set; }

        [ForeignKey("VerifiedByHROfficerID")]
        public virtual HROfficer VerifiedByHROfficer { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Verification Date")]
        public DateTime VerificationDate { get; set; }
    }
}