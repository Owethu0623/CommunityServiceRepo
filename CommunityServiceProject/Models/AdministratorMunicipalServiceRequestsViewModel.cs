
using System;
using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class AdministratorMunicipalServiceRequestsViewModel
    {
        public int TotalRequests { get; set; }
        public int SubmittedRequests { get; set; }
        public int UnderReviewRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }

        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public int? ServiceTypeFilter { get; set; }

        public List<AdministratorMunicipalServiceRequestListItemViewModel> Requests { get; set; }

        public List<ServiceTypeFilterItemViewModel> ServiceTypes { get; set; }

        public AdministratorMunicipalServiceRequestsViewModel()
        {
            Requests = new List<AdministratorMunicipalServiceRequestListItemViewModel>();
            ServiceTypes = new List<ServiceTypeFilterItemViewModel>();
        }
    }

    public class AdministratorMunicipalServiceRequestListItemViewModel
    {
        public int MunicipalServiceRequestID { get; set; }

        public string ReferenceNumber { get; set; }

        public string CitizenName { get; set; }

        public string CitizenEmail { get; set; }

        public string ServiceCode { get; set; }

        public string ServiceName { get; set; }

        public string Title { get; set; }

        public MunicipalServiceRequestStatus Status { get; set; }

        public DateTime DateSubmitted { get; set; }

        public DateTime? DateReviewed { get; set; }

        public DateTime? DateApproved { get; set; }
    }

    public class ServiceTypeFilterItemViewModel
    {
        public int ServiceTypeID { get; set; }

        public string ServiceName { get; set; }
    }
}

