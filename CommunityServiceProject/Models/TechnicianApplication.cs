using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class TechnicianApplication
    {
        [Key]
        public int ApplicationID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Application Reference")]
        public string ApplicationReference { get; set; }

        [Index(
    "IX_TechnicianApplication_Opportunity_Citizen",
    1,
    IsUnique = true)]
        [Required]
        public int OpportunityID { get; set; }

        [ForeignKey("OpportunityID")]
        public virtual TechnicianOpportunity Opportunity { get; set; }

        [Index(
            "IX_TechnicianApplication_Opportunity_Citizen",
            2,
            IsUnique = true)]
        [Required]
        public int CitizenID { get; set; }

        [ForeignKey("CitizenID")]
        public virtual Citizen Citizen { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Application Date")]
        public DateTime ApplicationDate { get; set; }

        [Required]
        [Display(Name = "Application Status")]
        public TechnicianApplicationStatus Status { get; set; }

        [Required]
        [StringLength(
            4000,
            MinimumLength = 50,
            ErrorMessage =
                "The cover letter must be between 50 and 4000 characters."
        )]
        [Display(Name = "Cover Letter")]
        public string CoverLetter { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Last Updated")]
        public DateTime? LastUpdatedDate { get; set; }
    }
}