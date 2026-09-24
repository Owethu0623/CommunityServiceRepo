using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class AssetMaintenanceHistoryViewModel
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

        public int TotalMaintenanceRecords { get; set; }

        public List<AssetMaintenanceHistoryItemViewModel> MaintenanceHistory { get; set; }

        public AssetMaintenanceHistoryViewModel()
        {
            MaintenanceHistory = new List<AssetMaintenanceHistoryItemViewModel>();
        }
    }


    public class AssetMaintenanceHistoryItemViewModel
    {
        public int AssetMaintenanceID { get; set; }

        public int MaintenanceWorkID { get; set; }

        [Display(Name = "Request Reference")]
        public string RequestReferenceNumber { get; set; }

        [Display(Name = "Request Title")]
        public string RequestTitle { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [Display(Name = "Technician")]
        public string TechnicianName { get; set; }

        [Display(Name = "Started")]
        public DateTime? StartedDate { get; set; }

        [Display(Name = "Completed")]
        public DateTime? CompletedDate { get; set; }

        public string MaintenanceStatus { get; set; }

        public string MaintenanceSummary { get; set; }

        public string ResolutionAction { get; set; }

        public DateTime SubmittedDate { get; set; }

        public string VerificationStatus { get; set; }

        public DateTime? VerifiedDate { get; set; }

        public string AdministratorComments { get; set; }

        public DateTime LinkDate { get; set; }

        public string LinkingNotes { get; set; }
    }
}