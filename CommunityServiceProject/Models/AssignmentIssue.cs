using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class AssignmentIssue
    {
        [Key]
        public int AssignmentIssueID { get; set; }

        // Assignment relationship
        [Required]
        public int AssignmentID { get; set; }

        public virtual TechnicianAssignment Assignment { get; set; }

        // Issue information
        [Required]
        public AssignmentIssueType IssueType { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }

        // Date the issue was reported
        [Required]
        public DateTime ReportedDate { get; set; }

        // Current issue status
        [Required]
        public AssignmentIssueStatus Status { get; set; }

        // Administrator response/resolution
        [StringLength(1000)]
        public string AdministratorResponse { get; set; }

        public DateTime? ResolvedDate { get; set; }
    }
}