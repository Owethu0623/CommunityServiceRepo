using System;

namespace CommunityServiceProject.ViewModels
{
    public class CitizenNotificationViewModel
    {
        public int NotificationID { get; set; }

        // Common notification information
        public string NotificationSource { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Category { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }


        // Generic related record information
        public string RelatedLabel { get; set; }

        public string RelatedReference { get; set; }


        // Maintenance
        public int? RequestID { get; set; }

        public string RequestReference { get; set; }


        // Technician Applications
        public int? ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string OpportunityTitle { get; set; }


        // Municipal Services
        public int? MunicipalServiceRequestID { get; set; }

        public string MunicipalServiceRequestReference { get; set; }


        // Finance
        public int? InvoiceID { get; set; }

        public string InvoiceReference { get; set; }

        public int? PaymentID { get; set; }

        public string TransactionReference { get; set; }

        public int? RefundID { get; set; }

        public int? ReceiptID { get; set; }

        
    }
}