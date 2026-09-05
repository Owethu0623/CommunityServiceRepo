using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommunityServiceProject.Models
{
    public enum MaintenanceWorkStatus
    {
        NotStarted,
        InProgress,
        SubmittedForVerification,
        Verified,
        Rejected
    }
}