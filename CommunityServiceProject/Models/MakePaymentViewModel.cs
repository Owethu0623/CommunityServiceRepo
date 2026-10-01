
using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class MakePaymentViewModel
    {
        public int InvoiceID { get; set; }

        public int MunicipalServiceRequestID { get; set; }

        public string InvoiceNumber { get; set; }

        public string RequestReference { get; set; }

        public string ServiceName { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal Balance { get; set; }

        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }
    }
}

