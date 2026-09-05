using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class MaintenanceMaterial
    {
        [Key]
        public int MaintenanceMaterialID { get; set; }

        // Maintenance work relationship
        [Required]
        public int MaintenanceWorkID { get; set; }

        public virtual MaintenanceWork MaintenanceWork { get; set; }

        // Material/resource information
        [Required]
        [StringLength(100)]
        public string MaterialName { get; set; }

        [Required]
        [Range(1, 100000)]
        public int Quantity { get; set; }

        [Required]
        [StringLength(30)]
        public string Unit { get; set; }

        // Date and time material/resource was recorded
        [Required]
        public DateTime RecordedDate { get; set; }
    }
}