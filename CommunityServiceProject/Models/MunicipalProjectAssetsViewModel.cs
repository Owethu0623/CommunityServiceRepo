using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectAssetsViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public int LinkedAssetCount { get; set; }

        public List<ProjectAssetItemViewModel> LinkedAssets { get; set; }

        public List<ProjectAssetAvailableItemViewModel> AvailableAssets { get; set; }

        public List<SelectListItem> AssetTypeOptions { get; set; }

        public List<SelectListItem> ConditionOptions { get; set; }

        public List<SelectListItem> StatusOptions { get; set; }

        public List<SelectListItem> WardOptions { get; set; }

        public string SearchTerm { get; set; }

        public string AssetTypeFilter { get; set; }

        public string ConditionFilter { get; set; }

        public string StatusFilter { get; set; }

        public int? WardFilter { get; set; }

        public string Notes { get; set; }

        public MunicipalProjectAssetsViewModel()
        {
            LinkedAssets =
                new List<ProjectAssetItemViewModel>();

            AvailableAssets =
                new List<ProjectAssetAvailableItemViewModel>();

            AssetTypeOptions =
                new List<SelectListItem>();

            ConditionOptions =
                new List<SelectListItem>();

            StatusOptions =
                new List<SelectListItem>();

            WardOptions =
                new List<SelectListItem>();
        }
    }

    public class ProjectAssetItemViewModel
    {
        public int AssetProjectID { get; set; }

        public int AssetID { get; set; }

        public string AssetCode { get; set; }

        public string AssetName { get; set; }

        public string AssetType { get; set; }

        public string AssetCategory { get; set; }

        public string WardName { get; set; }

        public string Condition { get; set; }

        public string Status { get; set; }

        public DateTime? LastInspectionDate { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }

        public DateTime LinkDate { get; set; }

        public string Notes { get; set; }

        public string LinkedByAdministratorName { get; set; }
    }

    public class ProjectAssetAvailableItemViewModel
    {
        public int AssetID { get; set; }

        public string AssetCode { get; set; }

        public string AssetName { get; set; }

        public string AssetType { get; set; }

        public string AssetCategory { get; set; }

        public string WardName { get; set; }

        public string Condition { get; set; }

        public string Status { get; set; }

        public DateTime DateRegistered { get; set; }

        public DateTime? LastInspectionDate { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }
    }

    public class LinkProjectAssetViewModel
    {
        [Required]
        public int ProjectID { get; set; }

        [Required]
        public int AssetID { get; set; }

        [StringLength(
            1000,
            ErrorMessage = "Notes cannot exceed 1000 characters.")]
        public string Notes { get; set; }
    }
}