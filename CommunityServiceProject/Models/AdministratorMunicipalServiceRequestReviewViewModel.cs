
using CommunityServiceProject.Models;
using System;

namespace CommunityServiceProject.ViewModels
{
    public class AdministratorMunicipalServiceRequestReviewViewModel
    {
        public int MunicipalServiceRequestID { get; set; }

        public string ReferenceNumber { get; set; }

        // Citizen information
        public int CitizenID { get; set; }
        public string CitizenName { get; set; }
        public string CitizenEmail { get; set; }
        public string CitizenPhoneNumber { get; set; }
        public string CitizenAddress { get; set; }

        // Service information
        public int ServiceTypeID { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }

        // Request information
        public string Title { get; set; }
        public string Description { get; set; }
        public string AdditionalInformation { get; set; }

        // Workflow information
        public MunicipalServiceRequestStatus Status { get; set; }

        public DateTime DateSubmitted { get; set; }
        public DateTime? DateReviewed { get; set; }
        public DateTime? DateApproved { get; set; }
        public DateTime? DateCompleted { get; set; }

        public string ReviewedByAdministratorName { get; set; }

        // Used by the review actions
        public string RejectionReason { get; set; }
    }
}

