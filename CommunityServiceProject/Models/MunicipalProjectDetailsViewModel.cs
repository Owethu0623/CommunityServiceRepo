using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectDetailsViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        [Display(Name = "Project Scope")]
        public string ProjectScope { get; set; }
        public string Description { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public decimal? EstimatedBudget { get; set; }

        public string ResponsibleAdministratorName { get; set; }

        public string ResponsibleAdministratorEmail { get; set; }

        public DateTime DateRegistered { get; set; }

        public string CreatedByAdministratorName { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public string LastUpdatedByAdministratorName { get; set; }

        public bool IsOverdue
        {
            get
            {
                return ExpectedCompletionDate.HasValue
                       && ExpectedCompletionDate.Value.Date < DateTime.Today
                       && Status != "Completed"
                       && Status != "Cancelled";
            }
        }

        public int? DaysRemaining
        {
            get
            {
                if (!ExpectedCompletionDate.HasValue)
                    return null;

                return (ExpectedCompletionDate.Value.Date - DateTime.Today).Days;
            }
        }
    }
}