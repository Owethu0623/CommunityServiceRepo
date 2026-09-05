using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianDashboardViewModel
    {
        public int AssignedCount { get; set; }

        public int InProgressCount { get; set; }

        public int CompletedCount { get; set; }

        public int AwaitingAcknowledgementCount { get; set; }

        public int ReassignmentRequestCount { get; set; }
    }
}