using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectRequestsViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string RelationshipType { get; set; }
        public string WardName { get; set; }

        public List<ProjectRequestItemViewModel> LinkedRequests { get; set; }

        public List<ProjectRequestAvailableItemViewModel> AvailableRequests { get; set; }

        public List<SelectListItem> RelationshipTypeOptions { get; set; }

        public List<SelectListItem> StatusOptions { get; set; }

        public List<SelectListItem> PriorityOptions { get; set; }

        public List<SelectListItem> WardOptions { get; set; }

        // Search/filter values
        public string SearchTerm { get; set; }

        public string StatusFilter { get; set; }

        public string PriorityFilter { get; set; }

        public int? WardFilter { get; set; }

        public int LinkedRequestCount { get; set; }

        public MunicipalProjectRequestsViewModel()
        {
            LinkedRequests = new List<ProjectRequestItemViewModel>();
            AvailableRequests = new List<ProjectRequestAvailableItemViewModel>();
            RelationshipTypeOptions = new List<SelectListItem>();
            StatusOptions = new List<SelectListItem>();
            PriorityOptions = new List<SelectListItem>();
            WardOptions = new List<SelectListItem>();
        }
    }

    public class ProjectRequestItemViewModel
    {
        public int ProjectRequestID { get; set; }

        public int RequestID { get; set; }

        public string ReferenceNumber { get; set; }

        public string RequestTitle { get; set; }

        public string CategoryName { get; set; }

        public string WardName { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }

        public DateTime DateSubmitted { get; set; }

        public string RelationshipType { get; set; }

        public string Notes { get; set; }

        public DateTime DateLinked { get; set; }

        public string LinkedByAdministratorName { get; set; }
    }

    public class ProjectRequestAvailableItemViewModel
    {
        public int RequestID { get; set; }

        public string ReferenceNumber { get; set; }

        public string RequestTitle { get; set; }

        public string CategoryName { get; set; }

        public string WardName { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }

        public DateTime DateSubmitted { get; set; }
    }

    public class LinkProjectRequestViewModel
    {
        [Required]
        public int ProjectID { get; set; }

        [Required(ErrorMessage = "Please select a service request.")]
        [Display(Name = "Service Request")]
        public int? RequestID { get; set; }

        [Required(ErrorMessage = "Please select a relationship type.")]
        [StringLength(100)]
        [Display(Name = "Relationship Type")]
        public string RelationshipType { get; set; }

        [StringLength(1000)]
        [Display(Name = "Administrator Notes")]
        public string Notes { get; set; }
    }
}