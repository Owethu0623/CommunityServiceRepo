using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class InspectMunicipalAssetViewModel
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

        [Required(ErrorMessage = "Please provide the inspection date.")]
        [Display(Name = "Inspection Date")]
        [DataType(DataType.Date)]
        public DateTime? InspectionDate { get; set; }

        [Required(ErrorMessage = "Please select the condition observed during inspection.")]
        [Display(Name = "Condition Observed")]
        public AssetCondition? ConditionObserved { get; set; }

        [Required(ErrorMessage = "Please provide the inspection findings.")]
        [StringLength(
            2000,
            MinimumLength = 5,
            ErrorMessage = "Inspection findings must be between 5 and 2000 characters."
        )]
        [Display(Name = "Inspection Findings")]
        public string Findings { get; set; }

        [StringLength(
            2000,
            ErrorMessage = "Inspector notes cannot exceed 2000 characters."
        )]
        [Display(Name = "Inspector Notes")]
        public string InspectorNotes { get; set; }

        [StringLength(
            100,
            ErrorMessage = "Recommended action cannot exceed 100 characters."
        )]
        [Display(Name = "Recommended Action")]
        public string RecommendedAction { get; set; }

        [Display(Name = "Next Inspection Date")]
        [DataType(DataType.Date)]
        public DateTime? NextInspectionDate { get; set; }
    }
}