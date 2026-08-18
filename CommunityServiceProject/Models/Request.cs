using CommunityServiceProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace CommunityServiceProject.Models
{
    public class Request
    {
        [Key]
        public int RequestID { get; set; }

        // Request information
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [StringLength(200)]
        public string LocationDescription { get; set; }

        // GPS Location
        // Location
        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Uploaded photo
        public string ImagePath { get; set; }

        // System information
        public DateTime DateSubmitted { get; set; }

        public RequestStatus Status { get; set; }

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


        // Business methods

        public void SubmitRequest()
        {
            DateSubmitted = DateTime.Now;
            Status = RequestStatus.Pending;
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
    }
}