
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class ServiceTypeManagementViewModel
    {
        public int TotalServices { get; set; }

        public int ActiveServices { get; set; }

        public int InactiveServices { get; set; }

        public int ChargeableServices { get; set; }

        public int NonChargeableServices { get; set; }

        public string SearchTerm { get; set; }

        public string StatusFilter { get; set; }

        public string ChargeableFilter { get; set; }

        public List<ServiceTypeListItemViewModel> Services { get; set; }

        public ServiceTypeManagementViewModel()
        {
            Services = new List<ServiceTypeListItemViewModel>();
        }
    }

    public class ServiceTypeListItemViewModel
    {
        public int ServiceTypeID { get; set; }

        public string ServiceCode { get; set; }

        public string ServiceName { get; set; }

        public string Description { get; set; }

        public bool IsChargeable { get; set; }

        public bool IsActive { get; set; }

        public int ServiceRequestCount { get; set; }
    }

    public class ServiceTypeFormViewModel
    {
        public int ServiceTypeID { get; set; }

        [Required(ErrorMessage = "Service code is required.")]
        [StringLength(30, ErrorMessage = "Service code cannot exceed 30 characters.")]
        [RegularExpression(
            @"^[A-Za-z0-9]+(-[A-Za-z0-9]+)*$",
            ErrorMessage = "Service code can contain letters, numbers and hyphens only."
        )]
        [Display(Name = "Service Code")]
        public string ServiceCode { get; set; }

        [Required(ErrorMessage = "Service name is required.")]
        [StringLength(150, ErrorMessage = "Service name cannot exceed 150 characters.")]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Chargeable Service")]
        public bool IsChargeable { get; set; }

        [Display(Name = "Active Service")]
        public bool IsActive { get; set; }
    }
}

