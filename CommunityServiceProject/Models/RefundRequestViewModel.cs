using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CommunityServiceProject.ViewModels
{
    public class RefundRequestViewModel
    {
        public int PaymentID { get; set; }

        public int InvoiceID { get; set; }

        public string TransactionReference { get; set; }

        public string InvoiceNumber { get; set; }

        public string RequestReference { get; set; }

        public string ServiceName { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal PaymentAmount { get; set; }

        [Required(ErrorMessage = "Please enter the amount you want refunded.")]
        [Range(0.01, 999999999, ErrorMessage = "The refund amount must be greater than zero.")]
        [Display(Name = "Refund Amount")]
        public decimal RefundAmount { get; set; }

        [Required(ErrorMessage = "Please provide a reason for the refund request.")]
        [StringLength(
            500,
            MinimumLength = 10,
            ErrorMessage = "The refund reason must be between 10 and 500 characters.")]
        [Display(Name = "Reason for Refund")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Please attach supporting documentation.")]
        [Display(Name = "Supporting Document")]
        public HttpPostedFileBase SupportingDocument { get; set; }
    }
}