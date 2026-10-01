
using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class GenerateInvoiceListViewModel
    {
        public int MunicipalServiceRequestID { get; set; }

        public string RequestReference { get; set; }

        public string CitizenName { get; set; }

        public string ServiceCode { get; set; }

        public string ServiceName { get; set; }

        public string FeeType { get; set; }

        public decimal Amount { get; set; }

        public DateTime? DateApproved { get; set; }

        public string RequestStatus { get; set; }

        public int FeeScheduleID { get; set; }
    }

    public class GenerateInvoiceViewModel
    {
        public int MunicipalServiceRequestID { get; set; }

        public string RequestReference { get; set; }

        public string CitizenName { get; set; }

        public string CitizenEmail { get; set; }

        public string ServiceCode { get; set; }

        public string ServiceName { get; set; }

        public string ServiceDescription { get; set; }

        public string FeeType { get; set; }

        public decimal Amount { get; set; }

        public int FeeScheduleID { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [Required(ErrorMessage = "Please select an invoice due date.")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
    }

    public class InvoiceDetailsViewModel
    {
        public int InvoiceID { get; set; }

        public string InvoiceNumber { get; set; }

        public string RequestReference { get; set; }

        public int MunicipalServiceRequestID { get; set; }

        public string CitizenName { get; set; }

        public string CitizenEmail { get; set; }

        public string ServiceCode { get; set; }

        public string ServiceName { get; set; }

        public string FeeType { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal Balance { get; set; }

        public InvoiceStatus Status { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? PaidDate { get; set; }
    }
}

