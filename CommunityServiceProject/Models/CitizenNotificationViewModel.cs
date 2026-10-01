using System;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace CommunityServiceProject.ViewModels
{
    public class CitizenNotificationViewModel
    {
        public int NotificationID { get; set; }

        public string NotificationSource { get; set; }
        public int? MunicipalServiceRequestID { get; set; }

        public string MunicipalServiceRequestReference { get; set; }
        public string Title { get; set; }

        public string Message { get; set; }

        public string Category { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }

        public int? ApplicationID { get; set; }

        public int? RequestID { get; set; }

        public string ApplicationReference { get; set; }

        public string OpportunityTitle { get; set; }

        public string RequestReference { get; set; }
    }
}
