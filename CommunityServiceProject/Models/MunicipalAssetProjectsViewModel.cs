
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalAssetProjectsViewModel
    {
        public int AssetID { get; set; }

        public string AssetCode { get; set; }

        public string AssetName { get; set; }

        public string AssetType { get; set; }

        public string AssetCategory { get; set; }

        public string WardName { get; set; }

        public string LocationDescription { get; set; }

        public List<AssetProjectLinkedItemViewModel> LinkedProjects { get; set; }

        public List<AssetProjectAvailableItemViewModel> AvailableProjects { get; set; }

        public List<SelectListItem> ProjectTypeOptions { get; set; }

        public List<SelectListItem> ProjectStatusOptions { get; set; }

        public List<SelectListItem> ProjectPriorityOptions { get; set; }

        public List<SelectListItem> WardOptions { get; set; }

        public string SearchTerm { get; set; }

        public string ProjectTypeFilter { get; set; }

        public string ProjectStatusFilter { get; set; }

        public string ProjectPriorityFilter { get; set; }

        public int? WardFilter { get; set; }

        public string Notes { get; set; }

        public MunicipalAssetProjectsViewModel()
        {
            LinkedProjects =
                new List<AssetProjectLinkedItemViewModel>();

            AvailableProjects =
                new List<AssetProjectAvailableItemViewModel>();

            ProjectTypeOptions =
                new List<SelectListItem>();

            ProjectStatusOptions =
                new List<SelectListItem>();

            ProjectPriorityOptions =
                new List<SelectListItem>();

            WardOptions =
                new List<SelectListItem>();
        }
    }


    public class AssetProjectLinkedItemViewModel
    {
        public int AssetProjectID { get; set; }

        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        public DateTime LinkDate { get; set; }

        public string Notes { get; set; }

        public string LinkedByAdministratorName { get; set; }
    }


    public class AssetProjectAvailableItemViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }
    }


    public class LinkAssetProjectViewModel
    {
        [Required]
        public int AssetID { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [StringLength(
            1000,
            ErrorMessage = "Notes cannot exceed 1000 characters.")]
        public string Notes { get; set; }
    }
}
