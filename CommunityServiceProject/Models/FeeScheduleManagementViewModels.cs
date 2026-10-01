
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class FeeScheduleManagementViewModel
    {
        public int TotalFees { get; set; }
        public int ActiveFees { get; set; }
        public int InactiveFees { get; set; }
        public decimal TotalActiveFeeValue { get; set; }

        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public string ServiceTypeFilter { get; set; }

        public List<FeeScheduleListItemViewModel> Fees { get; set; }

        public FeeScheduleManagementViewModel()
        {
            Fees = new List<FeeScheduleListItemViewModel>();
        }
    }

    public class FeeScheduleListItemViewModel
    {
        public int FeeScheduleID { get; set; }

        public int ServiceTypeID { get; set; }

        public string ServiceCode { get; set; }

        public string ServiceName { get; set; }

        public string FeeType { get; set; }

        public decimal Amount { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; }

        public string CreatedByName { get; set; }
    }

    public class FeeScheduleFormViewModel
    {
        public int FeeScheduleID { get; set; }

        [Required(ErrorMessage = "Service type is required.")]
        [Display(Name = "Service Type")]
        public int ServiceTypeID { get; set; }

        [Required(ErrorMessage = "Fee type is required.")]
        [StringLength(
            50,
            ErrorMessage = "Fee type cannot exceed 50 characters."
        )]
        [Display(Name = "Fee Type")]
        public string FeeType { get; set; }

        [Required(ErrorMessage = "Fee amount is required.")]
        [Range(
            0.01,
            999999999.99,
            ErrorMessage = "Fee amount must be greater than zero."
        )]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Effective from date is required.")]
        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; }

        [Display(Name = "Effective To")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [Display(Name = "Active Fee")]
        public bool IsActive { get; set; }
    }
}

