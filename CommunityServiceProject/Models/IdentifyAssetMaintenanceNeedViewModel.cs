using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class IdentifyAssetMaintenanceNeedViewModel
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

        [Required(ErrorMessage = "Please select the maintenance type.")]
        [StringLength(200)]
        [Display(Name = "Maintenance Type")]
        public string MaintenanceType { get; set; }

        [Required(ErrorMessage = "Please describe the maintenance requirement.")]
        [StringLength(
            2000,
            MinimumLength = 5,
            ErrorMessage = "The maintenance description must be between 5 and 2000 characters."
        )]
        [Display(Name = "Maintenance Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select the maintenance priority.")]
        [Display(Name = "Priority")]
        public AssetMaintenancePriority? Priority { get; set; }

        [Display(Name = "Target Date")]
        [DataType(DataType.Date)]
        public DateTime? TargetDate { get; set; }

        public List<SelectListItem> MaintenanceTypeOptions { get; set; }

        public IdentifyAssetMaintenanceNeedViewModel()
        {
            MaintenanceTypeOptions = new List<SelectListItem>();
        }
    }
}