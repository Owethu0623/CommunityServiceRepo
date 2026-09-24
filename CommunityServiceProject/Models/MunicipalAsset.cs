using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class MunicipalAsset
    {
        [Key]
        public int AssetID { get; set; }

        [Required]
        [StringLength(50)]
        [Index("IX_MunicipalAsset_AssetCode", IsUnique = true)]
        [Display(Name = "Asset Code")]
        public string AssetCode { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Asset Name")]
        public string AssetName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Asset Type")]
        public string AssetType { get; set; }

        [StringLength(100)]
        [Display(Name = "Asset Category")]
        public string AssetCategory { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        // =========================================================
        // LOCATION
        // =========================================================

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }

        [StringLength(500)]
        [Display(Name = "Location Description")]
        public string LocationDescription { get; set; }

        [Required]
        [Display(Name = "Ward")]
        public int WardID { get; set; }

        [ForeignKey("WardID")]
        public virtual Ward Ward { get; set; }

        // =========================================================
        // CONDITION / STATUS
        // =========================================================

        [Required]
        [Display(Name = "Condition")]
        public AssetCondition Condition { get; set; }

        [Required]
        [Display(Name = "Status")]
        public AssetStatus Status { get; set; }

        // =========================================================
        // LIFECYCLE
        // =========================================================

        [Required]
        [Display(Name = "Date Registered")]
        public DateTime DateRegistered { get; set; }

        [Display(Name = "Last Inspection")]
        public DateTime? LastInspectionDate { get; set; }

        [Display(Name = "Last Maintenance")]
        public DateTime? LastMaintenanceDate { get; set; }

        [Display(Name = "Date Retired")]
        public DateTime? DateRetired { get; set; }

        [StringLength(1000)]
        [Display(Name = "Retirement Reason")]
        public string RetirementReason { get; set; }

        // =========================================================
        // AUDIT
        // =========================================================

        [Required]
        public int CreatedByAdministratorID { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public int? LastUpdatedByAdministratorID { get; set; }

        // =========================================================
        // NAVIGATION PROPERTIES
        // =========================================================

        [ForeignKey("CreatedByAdministratorID")]
        public virtual Administrator CreatedByAdministrator { get; set; }

        [ForeignKey("LastUpdatedByAdministratorID")]
        public virtual Administrator LastUpdatedByAdministrator { get; set; }

        public virtual ICollection<AssetInspection> AssetInspections { get; set; }

        public virtual ICollection<AssetMaintenanceNeed> MaintenanceNeeds { get; set; }

        public virtual ICollection<AssetRequest> AssetRequests { get; set; }

        public virtual ICollection<AssetMaintenance> AssetMaintenanceRecords { get; set; }

        public virtual ICollection<AssetProject> AssetProjects { get; set; }

        public virtual ICollection<AssetHistory> AssetHistory { get; set; }

        public MunicipalAsset()
        {
            AssetInspections = new HashSet<AssetInspection>();
            MaintenanceNeeds = new HashSet<AssetMaintenanceNeed>();
            AssetRequests = new HashSet<AssetRequest>();
            AssetMaintenanceRecords = new HashSet<AssetMaintenance>();
            AssetProjects = new HashSet<AssetProject>();
            AssetHistory = new HashSet<AssetHistory>();
        }
    }
}