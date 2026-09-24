using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class AssetHistoryViewModel
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

        public string SearchTerm { get; set; }

        public string SelectedActivityType { get; set; }

        public int TotalHistoryRecords { get; set; }

        public List<string> ActivityTypes { get; set; }

        public List<AssetHistoryItemViewModel> History { get; set; }

        public AssetHistoryViewModel()
        {
            ActivityTypes = new List<string>();
            History = new List<AssetHistoryItemViewModel>();
        }
    }

    public class AssetHistoryItemViewModel
    {
        public int AssetHistoryID { get; set; }

        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; }

        [Display(Name = "Activity")]
        public string ActivityType { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Previous Value")]
        public string PreviousValue { get; set; }

        [Display(Name = "New Value")]
        public string NewValue { get; set; }

        [Display(Name = "Administrator")]
        public string AdministratorName { get; set; }
    }
}