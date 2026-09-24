using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CommunityServiceProject.ViewModels
{
    public class LinkAssetMaintenanceViewModel
    {
        // -------------------------------------------------------
        // Asset information
        // -------------------------------------------------------

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


        // -------------------------------------------------------
        // Maintenance selection
        // -------------------------------------------------------

        [Required(ErrorMessage = "Please select a completed maintenance work record.")]
        [Display(Name = "Completed Maintenance Work")]
        public int? MaintenanceWorkID { get; set; }

        public List<SelectListItem> MaintenanceWorkOptions { get; set; }


        // -------------------------------------------------------
        // Administrator notes
        // -------------------------------------------------------

        [StringLength(
            1000,
            ErrorMessage = "Linking notes cannot exceed 1000 characters."
        )]
        [Display(Name = "Linking Notes")]
        public string Notes { get; set; }


        public LinkAssetMaintenanceViewModel()
        {
            MaintenanceWorkOptions = new List<SelectListItem>();
        }
    }
}