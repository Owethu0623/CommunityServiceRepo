using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class AssetMaintenance
    {
        [Key]
        public int AssetMaintenanceID { get; set; }

        [Required]
        public int AssetID { get; set; }

        [Required]
        public int MaintenanceWorkID { get; set; }

        [Required]
        public int LinkedByAdministratorID { get; set; }

        [Required]
        public DateTime LinkDate { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [ForeignKey("AssetID")]
        public virtual MunicipalAsset Asset { get; set; }

        [ForeignKey("MaintenanceWorkID")]
        public virtual MaintenanceWork MaintenanceWork { get; set; }

        [ForeignKey("LinkedByAdministratorID")]
        public virtual Administrator LinkedByAdministrator { get; set; }
    }
}