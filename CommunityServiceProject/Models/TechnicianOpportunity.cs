using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class TechnicianOpportunity
    {
        [Key]
        public int OpportunityID { get; set; }

        [Required]
        [StringLength(50)]
        [Index("IX_TechnicianOpportunity_OpportunityCode", IsUnique = true)]
        [Display(Name = "Opportunity Code")]
        public string OpportunityCode { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 5)]
        [Display(Name = "Opportunity Title")]
        public string Title { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 20)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 20)]
        [Display(Name = "Responsibilities")]
        public string Responsibilities { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 20)]
        [Display(Name = "Requirements")]
        public string Requirements { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        [Display(Name = "Required Qualifications")]
        public string RequiredQualifications { get; set; }

        [StringLength(2000)]
        [Display(Name = "Required Experience")]
        public string RequiredExperience { get; set; }

        [StringLength(2000)]
        [Display(Name = "Application Instructions")]
        public string ApplicationInstructions { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Application Start Date")]
        public DateTime ApplicationStartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Application Deadline")]
        public DateTime ApplicationDeadline { get; set; }

        [Required]
        [Display(Name = "Status")]
        public TechnicianOpportunityStatus Status { get; set; }

        [StringLength(100)]
        [Display(Name = "Employment Type")]
        public string EmploymentType { get; set; }

        [Range(1, 999)]
        [Display(Name = "Number of Positions")]
        public int? NumberOfPositions { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }

        [Required]
        [Display(Name = "Created By")]
        public int CreatedByAdministratorID { get; set; }

        [ForeignKey("CreatedByAdministratorID")]
        public virtual Administrator CreatedByAdministrator { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Published Date")]
        public DateTime? PublishedDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Closed Date")]
        public DateTime? ClosedDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Last Updated Date")]
        public DateTime? LastUpdatedDate { get; set; }

        public int? LastUpdatedByAdministratorID { get; set; }

        [ForeignKey("LastUpdatedByAdministratorID")]
        public virtual Administrator LastUpdatedByAdministrator { get; set; }
    }

    public enum TechnicianOpportunityStatus
    {
        Draft,
        Published,
        Closed,
        Cancelled
    }
}