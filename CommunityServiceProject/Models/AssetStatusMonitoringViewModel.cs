using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class AssetStatusMonitoringViewModel
    {
        public string SearchTerm { get; set; }

        public string SelectedStatus { get; set; }

        public string SelectedCondition { get; set; }

        public string SelectedWard { get; set; }

        public int TotalAssets { get; set; }

        public int ActiveAssets { get; set; }

        public int UnderMaintenanceAssets { get; set; }

        public int AttentionAssets { get; set; }

        public int InactiveAssets { get; set; }

        public int RetiredAssets { get; set; }

        public List<AssetStatusMonitoringItemViewModel> Assets { get; set; }

        public List<string> StatusOptions { get; set; }

        public List<string> ConditionOptions { get; set; }

        public List<string> WardOptions { get; set; }

        public AssetStatusMonitoringViewModel()
        {
            Assets = new List<AssetStatusMonitoringItemViewModel>();
            StatusOptions = new List<string>();
            ConditionOptions = new List<string>();
            WardOptions = new List<string>();
        }
    }

    public class AssetStatusMonitoringItemViewModel
    {
        public int AssetID { get; set; }

        [Display(Name = "Asset Code")]
        public string AssetCode { get; set; }

        [Display(Name = "Asset Name")]
        public string AssetName { get; set; }

        [Display(Name = "Asset Type")]
        public string AssetType { get; set; }

        [Display(Name = "Asset Category")]
        public string AssetCategory { get; set; }

        [Display(Name = "Ward")]
        public string WardName { get; set; }

        [Display(Name = "Condition")]
        public string Condition { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Last Inspection")]
        public DateTime? LastInspectionDate { get; set; }

        [Display(Name = "Last Maintenance")]
        public DateTime? LastMaintenanceDate { get; set; }

        [Display(Name = "Date Registered")]
        public DateTime DateRegistered { get; set; }
    }
}