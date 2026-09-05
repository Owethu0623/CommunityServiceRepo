using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommunityServiceProject.Models
{
    public enum AssignmentStatus
    {
        PendingAcknowledgement,
        Acknowledged,
        IssueReported,
        ReassignmentRequested,
        Reassigned,
        Completed,
        Cancelled
    }
}