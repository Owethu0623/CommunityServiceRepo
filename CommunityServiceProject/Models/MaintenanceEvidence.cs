using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class MaintenanceEvidence
    {
        [Key]
        public int EvidenceID { get; set; }

        // Maintenance work relationship
        [Required]
        public int MaintenanceWorkID { get; set; }

        public virtual MaintenanceWork MaintenanceWork { get; set; }

        // Type of evidence
        [Required]
        public EvidenceType EvidenceType { get; set; }

        // Uploaded file
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        // Optional description
        [StringLength(500)]
        public string Description { get; set; }

        // Upload date and time
        [Required]
        public DateTime UploadedDate { get; set; }
    }
}