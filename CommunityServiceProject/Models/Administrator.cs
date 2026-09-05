using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CommunityServiceProject.Models
{
    public class Administrator
    {
        [Key]
        public int AdministratorID { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string EmailAddress { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long."
        )]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, and a number."
        )]
        public string Password { get; set; }

        [Required]
        public AccountStatus AccountStatus { get; set; }

        // Requests reviewed/managed by this administrator
        public virtual ICollection<Request> Requests { get; set; }

        // Technician assignments created by this administrator
        public virtual ICollection<TechnicianAssignment> TechnicianAssignments { get; set; }

        // Reassignment requests reviewed by this administrator
        public virtual ICollection<ReassignmentRequest> ReassignmentRequests { get; set; }

        // Maintenance completions verified by this administrator
        public virtual ICollection<MaintenanceCompletion> MaintenanceCompletions { get; set; }

        public Administrator()
        {
            Requests = new List<Request>();
            TechnicianAssignments = new List<TechnicianAssignment>();
            ReassignmentRequests = new List<ReassignmentRequest>();
            MaintenanceCompletions = new List<MaintenanceCompletion>();
        }
    }
}