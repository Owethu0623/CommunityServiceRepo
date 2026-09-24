
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalAssetCreateViewModel
    {
        // =========================================================
        // ASSET CLASSIFICATION
        // =========================================================

        [Required(ErrorMessage = "Please select an asset type.")]
        [Display(Name = "Asset Type")]
        public string AssetType { get; set; }

        [Required(ErrorMessage = "Please select an asset category.")]
        [Display(Name = "Asset Category")]
        public string AssetCategory { get; set; }

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public IEnumerable<SelectListItem> AssetTypeOptions { get; set; }

        public IEnumerable<SelectListItem> AssetCategoryOptions { get; set; }


        // =========================================================
        // LOCATION
        // =========================================================

        [Display(Name = "Latitude")]
        public string Latitude { get; set; }

        [Display(Name = "Longitude")]
        public string Longitude { get; set; }

        [StringLength(500)]
        [Display(Name = "Location Description")]
        public string LocationDescription { get; set; }


        // =========================================================
        // MUNICIPAL ADMINISTRATIVE LOCATION
        // =========================================================

        [Required(ErrorMessage = "Please select the municipal ward.")]
        [Display(Name = "Ward")]
        public int? WardID { get; set; }

        public IEnumerable<SelectListItem> WardOptions { get; set; }


        // =========================================================
        // INITIAL OPERATIONAL INFORMATION
        // =========================================================

        [Required(ErrorMessage = "Please select the asset condition.")]
        [Display(Name = "Asset Condition")]
        public AssetCondition? Condition { get; set; }

        [Required(ErrorMessage = "Please select the asset status.")]
        [Display(Name = "Asset Status")]
        public AssetStatus? Status { get; set; }


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MunicipalAssetCreateViewModel()
        {
            AssetTypeOptions = new List<SelectListItem>();
            AssetCategoryOptions = new List<SelectListItem>();
            WardOptions = new List<SelectListItem>();
        }
    }
}

