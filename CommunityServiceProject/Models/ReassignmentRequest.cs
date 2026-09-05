using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class ReassignmentRequest
    {
        [Key]
        public int ReassignmentRequestID { get; set; }

        // Assignment being requested for reassignment
        [Required]
        public int AssignmentID { get; set; }

        public virtual TechnicianAssignment Assignment { get; set; }

        // Technician requesting reassignment
        [Required]
        public int TechnicianID { get; set; }

        public virtual Technician Technician { get; set; }

        // Reason for requesting reassignment
        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }

        // Request information
        [Required]
        public DateTime RequestedDate { get; set; }

        [Required]
        public ReassignmentStatus Status { get; set; }

        // Administrator review
        public int? ReviewedByAdministratorID { get; set; }

        public virtual Administrator ReviewedByAdministrator { get; set; }

        public DateTime? ReviewedDate { get; set; }

        [StringLength(1000)]
        public string AdministratorResponse { get; set; }
    }
}