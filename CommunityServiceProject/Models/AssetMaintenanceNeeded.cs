using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class AssetMaintenanceNeed
    {
        [Key]
        public int MaintenanceNeedID { get; set; }

        [Required]
        public int AssetID { get; set; }

        [Required]
        public int IdentifiedByAdministratorID { get; set; }

        [Required]
        [StringLength(200)]
        public string MaintenanceType { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        public AssetMaintenanceNeedStatus Status { get; set; }

        public AssetMaintenancePriority Priority { get; set; }

        [Required]
        public DateTime DateIdentified { get; set; }

        public DateTime? TargetDate { get; set; }

        public DateTime? ResolvedDate { get; set; }

        [StringLength(2000)]
        public string ResolutionNotes { get; set; }

        [ForeignKey("AssetID")]
        public virtual MunicipalAsset Asset { get; set; }

        [ForeignKey("IdentifiedByAdministratorID")]
        public virtual Administrator IdentifiedByAdministrator { get; set; }
    }
}