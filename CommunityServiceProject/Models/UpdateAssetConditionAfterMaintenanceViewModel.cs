using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class UpdateAssetConditionAfterMaintenanceViewModel
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

        [Display(Name = "Current Condition")]
        public AssetCondition CurrentCondition { get; set; }

        [Required(ErrorMessage = "Please select the completed maintenance work.")]
        [Display(Name = "Completed Maintenance Work")]
        public int? MaintenanceWorkID { get; set; }

        public List<SelectListItem> MaintenanceWorkOptions { get; set; }

        [Required(ErrorMessage = "Please select the condition after maintenance.")]
        [Display(Name = "Condition After Maintenance")]
        public AssetCondition? ConditionAfterMaintenance { get; set; }

        [Required(ErrorMessage = "Please provide the post-maintenance assessment.")]
        [StringLength(
            2000,
            MinimumLength = 5,
            ErrorMessage = "The assessment must be between 5 and 2000 characters."
        )]
        [Display(Name = "Post-Maintenance Assessment")]
        public string Assessment { get; set; }

        [StringLength(
            1000,
            ErrorMessage = "Additional notes cannot exceed 1000 characters."
        )]
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }

        public UpdateAssetConditionAfterMaintenanceViewModel()
        {
            MaintenanceWorkOptions = new List<SelectListItem>();
        }
    }
}