using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class MaintenanceWork
    {
        [Key]
        public int MaintenanceWorkID { get; set; }

        // Request being worked on
        [Required]
        public int RequestID { get; set; }

        public virtual Request Request { get; set; }

        // Technician performing the maintenance
        [Required]
        public int TechnicianID { get; set; }

        public virtual Technician Technician { get; set; }

        // Maintenance timing
        public DateTime? StartedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        // Current maintenance status
        [Required]
        public MaintenanceWorkStatus Status { get; set; }

        // Current progress
        [Range(0, 100)]
        public int ProgressPercentage { get; set; }

        [StringLength(500)]
        public string CurrentActivity { get; set; }

        // Related maintenance records
        public virtual ICollection<MaintenanceProgress> ProgressRecords { get; set; }

        public virtual ICollection<WorkNote> WorkNotes { get; set; }

        public virtual ICollection<MaintenanceMaterial> Materials { get; set; }

        public virtual ICollection<MaintenanceEvidence> Evidence { get; set; }

        public virtual ICollection<MaintenanceCompletion> Completions { get; set; }

        // Constructor
        public MaintenanceWork()
        {
            ProgressRecords = new List<MaintenanceProgress>();
            WorkNotes = new List<WorkNote>();
            Materials = new List<MaintenanceMaterial>();
            Evidence = new List<MaintenanceEvidence>();
            Completions = new List<MaintenanceCompletion>();
        }
    }
}