using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class MaintenanceProgress
    {
        [Key]
        public int ProgressID { get; set; }

        // Maintenance work relationship
        [Required]
        public int MaintenanceWorkID { get; set; }

        public virtual MaintenanceWork MaintenanceWork { get; set; }

        // Progress information
        [Required]
        [Range(0, 100)]
        public int ProgressPercentage { get; set; }

        [Required]
        [StringLength(500)]
        public string CurrentActivity { get; set; }

        // Date and time progress was recorded
        [Required]
        public DateTime RecordedDate { get; set; }
    }
}