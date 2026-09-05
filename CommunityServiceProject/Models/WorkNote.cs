using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class WorkNote
    {
        [Key]
        public int WorkNoteID { get; set; }

        // Maintenance work relationship
        [Required]
        public int MaintenanceWorkID { get; set; }

        public virtual MaintenanceWork MaintenanceWork { get; set; }

        // Work note
        [Required]
        [StringLength(1000)]
        public string NoteText { get; set; }

        // Date and time the note was recorded
        [Required]
        public DateTime CreatedDate { get; set; }
    }
}