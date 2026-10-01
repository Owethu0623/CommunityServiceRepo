using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace CommunityServiceProject.Models
{
    public class MunicipalServiceRequest
    {
        [Key]
        public int MunicipalServiceRequestID { get; set; }

        [Required]
        [StringLength(30)]
        [Index(IsUnique = true)]
        public string ReferenceNumber { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [Required]
        public int ServiceTypeID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(500)]
        public string AdditionalInformation { get; set; }

        [Required]
        public MunicipalServiceRequestStatus Status { get; set; }

        [Required]
        public DateTime DateSubmitted { get; set; }

        public DateTime? DateReviewed { get; set; }

        public DateTime? DateApproved { get; set; }

        public DateTime? DateCompleted { get; set; }

        public int? ReviewedByAdministratorID { get; set; }

        public virtual Citizen Citizen { get; set; }

        public virtual ServiceType ServiceType { get; set; }

        public virtual Administrator ReviewedByAdministrator { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }

        [AllowHtml]
        [StringLength(2000)]
        public string RejectionReason { get; set; }




        public MunicipalServiceRequest()
        {
            Invoices = new HashSet<Invoice>();
        }
        public void Submit()
        {
            DateSubmitted = DateTime.Now;
            Status = MunicipalServiceRequestStatus.Submitted;
        }
    }
}