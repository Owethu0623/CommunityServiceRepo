using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace CommunityServiceProject.Models
{
    public class Appeal
    {
        [Key]
        public int AppealID { get; set; }

        [Required]
        [StringLength(30)]
        public string ReferenceNumber { get; set; }

        [Required]
        public int CitizenID { get; set; }

        public virtual Citizen Citizen { get; set; }

        [Required]
        public int RestrictionID { get; set; }

        public virtual AccountRestriction Restriction { get; set; }

        public int? AdministratorID { get; set; }

        public virtual Administrator Administrator { get; set; }

        [Required]
        [StringLength(2000)]
        public string Reason { get; set; }

        [StringLength(500)]
        public string SupportingDocumentPath { get; set; }

        [Required]
        public DateTime DateSubmitted { get; set; }

        public DateTime? DateReviewed { get; set; }

        public DateTime? DateDecision { get; set; }

        [Required]
        public AppealStatus Status { get; set; }

        public bool ApprovalAcknowledged { get; set; }

        [StringLength(2000)]
        public string DecisionReason { get; set; }
    }
}

