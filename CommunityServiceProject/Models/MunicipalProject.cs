using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class MunicipalProject
    {
        [Key]
        public int ProjectID { get; set; }

        [Required]
        [StringLength(50)]
        [Index("IX_MunicipalProject_ProjectCode", IsUnique = true)]
        [Display(Name = "Project Code")]
        public string ProjectCode { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Project Type")]
        public string ProjectType { get; set; }

        [StringLength(2000)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Project Location")]
        public string ProjectLocation { get; set; }

        [Range(-90, 90)]
        [Display(Name = "Latitude")]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        [Display(Name = "Longitude")]
        public double? Longitude { get; set; }

        [Required]
        [Display(Name = "Ward")]
        public int WardID { get; set; }

        [ForeignKey("WardID")]
        public virtual Ward Ward { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Expected Completion Date")]
        public DateTime? ExpectedCompletionDate { get; set; }

        [Display(Name = "Actual Completion Date")]
        public DateTime? ActualCompletionDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public MunicipalProjectStatus Status { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public MunicipalProjectPriority Priority { get; set; }

        [Column(TypeName = "decimal")]
        [Range(0, 9999999999999999.99)]
        [Display(Name = "Estimated Budget")]
        public decimal? EstimatedBudget { get; set; }

        [Required]
        [Display(Name = "Responsible Administrator")]
        public int ResponsibleAdministratorID { get; set; }

        [ForeignKey("ResponsibleAdministratorID")]
        public virtual Administrator ResponsibleAdministrator { get; set; }

        [Required]
        [Display(Name = "Date Registered")]
        public DateTime DateRegistered { get; set; }

        [Required]
        [Display(Name = "Created By Administrator")]
        public int CreatedByAdministratorID { get; set; }

        [ForeignKey("CreatedByAdministratorID")]
        public virtual Administrator CreatedByAdministrator { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdatedDate { get; set; }

        public int? LastUpdatedByAdministratorID { get; set; }

        [ForeignKey("LastUpdatedByAdministratorID")]
        public virtual Administrator LastUpdatedByAdministrator { get; set; }

        /*
         * Relationships for later project-management stories.
         */

        public virtual ICollection<AssetProject> AssetProjects { get; set; }

        public MunicipalProject()
        {
            AssetProjects = new HashSet<AssetProject>();
        }
    }
}