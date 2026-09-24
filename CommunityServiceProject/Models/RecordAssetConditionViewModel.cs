using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class RecordAssetConditionViewModel
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

        [Required(ErrorMessage = "Please select the new asset condition.")]
        [Display(Name = "New Condition")]
        public AssetCondition? NewCondition { get; set; }

        [Required(ErrorMessage = "Please provide the assessment findings.")]
        [StringLength(
            2000,
            MinimumLength = 5,
            ErrorMessage = "Assessment findings must be between 5 and 2000 characters."
        )]
        [Display(Name = "Assessment Findings")]
        public string Findings { get; set; }

        [StringLength(
            100,
            ErrorMessage = "Recommended action cannot exceed 100 characters."
        )]
        [Display(Name = "Recommended Action")]
        public string RecommendedAction { get; set; }
    }
}