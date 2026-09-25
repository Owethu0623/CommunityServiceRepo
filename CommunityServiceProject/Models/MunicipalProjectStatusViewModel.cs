using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectStatusViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public MunicipalProjectStatus CurrentStatus { get; set; }

        [Required(ErrorMessage = "Please select the new project status.")]
        [Display(Name = "New Project Status")]
        public MunicipalProjectStatus NewStatus { get; set; }

        [Required(ErrorMessage = "Please provide a reason for the status change.")]
        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "The status change reason must be between 10 and 2000 characters."
        )]
        [Display(Name = "Reason for Status Change")]
        public string StatusChangeReason { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public string LastUpdatedByAdministratorName { get; set; }

        public List<MunicipalProjectStatusOptionViewModel> AvailableStatuses { get; set; }

        public MunicipalProjectStatusViewModel()
        {
            AvailableStatuses =
                new List<MunicipalProjectStatusOptionViewModel>();
        }
    }


    public class MunicipalProjectStatusOptionViewModel
    {
        public MunicipalProjectStatus Status { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }
    }
}