using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Request
    {
        [Key]
        public int RequestID { get; set; }

        [Required]
        [Display(Name = "Request Reference")]
        [StringLength(30)]
        [Index("IX_Request_ReferenceNumber", IsUnique = true)]
        public string ReferenceNumber { get; set; }

        // Request information
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        // GPS Location
        [Required]
        public double? Latitude { get; set; }

        [Required]
        public double? Longitude { get; set; }

        // Problem location
        [Required]
        [StringLength(500)]
        public string ProblemLocation { get; set; }

        // Uploaded photo
        public string ImagePath { get; set; }

        // System information
        public DateTime DateSubmitted { get; set; }

        public RequestStatus Status { get; set; }

        // Request priority
        public Priority Priority { get; set; }

        [StringLength(500)]
        public string PriorityReason { get; set; }

        // Citizen relationship
        [Required]
        public int CitizenID { get; set; }

        public virtual Citizen Citizen { get; set; }

        // Administrator relationship
        public int? AdministratorID { get; set; }

        public virtual Administrator Administrator { get; set; }

        // Technician relationship
        public int? TechnicianID { get; set; }

        public virtual Technician Technician { get; set; }

        // Category relationship
        [Required]
        public int CategoryID { get; set; }

        public virtual Category Category { get; set; }

        // Required skills
        public virtual ICollection<RequestSkill> RequiredSkills { get; set; }

        // Technician assignment history
        public virtual ICollection<TechnicianAssignment> TechnicianAssignments { get; set; }

        // Maintenance work records
        public virtual ICollection<MaintenanceWork> MaintenanceWorks { get; set; }

        // Ward relationship
        [Required]
        public int WardID { get; set; }

        public virtual Ward Ward { get; set; }

        // Business methods
        public void SubmitRequest()
        {
            DateSubmitted = DateTime.Now;
            Status = RequestStatus.Pending;
            Priority = Priority.Medium;
        }

        public bool CanEdit()
        {
            return Status == RequestStatus.Pending;
        }

        public int DaysOpen()
        {
            return (DateTime.Now - DateSubmitted).Days;
        }

        public bool IsCompleted()
        {
            return Status == RequestStatus.Completed;
        }

        public Request()
        {
            RequiredSkills = new List<RequestSkill>();
            TechnicianAssignments = new List<TechnicianAssignment>();
            MaintenanceWorks = new List<MaintenanceWork>();
        }
    }
}