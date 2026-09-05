using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class TechnicianAssignment
    {
        [Key]
        public int AssignmentID { get; set; }

        // Request being assigned
        [Required]
        public int RequestID { get; set; }

        public virtual Request Request { get; set; }

        // Technician receiving the assignment
        [Required]
        public int TechnicianID { get; set; }

        public virtual Technician Technician { get; set; }

        // Administrator who made the assignment
        [Required]
        public int AdministratorID { get; set; }

        public virtual Administrator Administrator { get; set; }

        // Assignment information
        [Required]
        public DateTime AssignedDate { get; set; }

        public DateTime? AcknowledgedDate { get; set; }

        // Current assignment status
        [Required]
        public AssignmentStatus Status { get; set; }

        // Assignment issues
        public virtual ICollection<AssignmentIssue> AssignmentIssues { get; set; }

        // Reassignment requests
        public virtual ICollection<ReassignmentRequest> ReassignmentRequests { get; set; }

        public TechnicianAssignment()
        {
            AssignmentIssues = new List<AssignmentIssue>();
            ReassignmentRequests = new List<ReassignmentRequest>();
        }
    }
}