using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace CommunityServiceProject.Models
{
    public enum RequestStatus
    {
        Pending,
        UnderReview,
        Approved,
        Assigned,
        InProgress,
        Completed,
        Rejected
    }
}