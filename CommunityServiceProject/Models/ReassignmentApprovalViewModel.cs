using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CommunityServiceProject.ViewModels
{
    public class ReassignmentApprovalViewModel
    {
        public int ReassignmentRequestID { get; set; }

        public int RequestID { get; set; }

        public string RequestTitle { get; set; }

        public string CategoryName { get; set; }

        public string WardName { get; set; }

        public string Priority { get; set; }

        public string CurrentTechnicianName { get; set; }

        public string Reason { get; set; }

        public System.DateTime RequestedDate { get; set; }

        [Required(ErrorMessage = "Please select a replacement technician.")]
        [Display(Name = "Replacement Technician")]
        public int? ReplacementTechnicianID { get; set; }

        public IEnumerable<SelectListItem> AvailableTechnicians { get; set; }

        [StringLength(1000)]
        [Display(Name = "Administrator Response")]
        public string AdministratorResponse { get; set; }
    }
}