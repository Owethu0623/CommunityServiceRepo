using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalAssetEditViewModel
    {
        public int AssetID { get; set; }

        [Display(Name = "Asset Code")]
        public string AssetCode { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Asset Name")]
        public string AssetName { get; set; }

        [Required]
        [Display(Name = "Asset Type")]
        public string AssetType { get; set; }

        [Required]
        [Display(Name = "Asset Category")]
        public string AssetCategory { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Ward")]
        public int WardID { get; set; }

        [Required]
        [Display(Name = "Location Description")]
        [StringLength(500)]
        public string LocationDescription { get; set; }

        [Required]
        [Display(Name = "Latitude")]
        public string Latitude { get; set; }

        [Required]
        [Display(Name = "Longitude")]
        public string Longitude { get; set; }

        [Required]
        public AssetCondition Condition { get; set; }

        [Required]
        public AssetStatus Status { get; set; }

        public List<SelectListItem> AssetTypeOptions { get; set; }

        public List<SelectListItem> AssetCategoryOptions { get; set; }

        public List<SelectListItem> WardOptions { get; set; }

        public MunicipalAssetEditViewModel()
        {
            AssetTypeOptions = new List<SelectListItem>();
            AssetCategoryOptions = new List<SelectListItem>();
            WardOptions = new List<SelectListItem>();
        }
    }
}