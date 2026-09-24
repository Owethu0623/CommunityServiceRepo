using CommunityServiceProject.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class RetireMunicipalAssetViewModel
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

        [Display(Name = "Current Status")]
        public AssetStatus CurrentStatus { get; set; }

        [Required(ErrorMessage = "Please provide a reason for retiring this asset.")]
        [StringLength(
            1000,
            MinimumLength = 5,
            ErrorMessage = "The retirement reason must be between 5 and 1000 characters."
        )]
        [Display(Name = "Retirement Reason")]
        public string RetirementReason { get; set; }

        [Required(ErrorMessage = "Please confirm that you want to retire this asset.")]
        [Display(Name = "Confirmation")]
        public bool ConfirmRetirement { get; set; }
    }
}