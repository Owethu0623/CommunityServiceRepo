using CommunityServiceProject.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CommunityServiceProject.ViewModels
{
    public class RequestClassificationViewModel
    {
        public int RequestID { get; set; }

        public string Title { get; set; }

        public string CategoryName { get; set; }

        public string ProblemLocation { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [Required(ErrorMessage = "Please select a ward.")]
        [Display(Name = "Ward")]
        public int WardID { get; set; }

        [Required(ErrorMessage = "Please select a priority.")]
        [Display(Name = "Priority")]
        public Priority Priority { get; set; }

        [Required(ErrorMessage = "Please provide a reason for the selected priority.")]
        [StringLength(500)]
        [Display(Name = "Priority Reason")]
        public string PriorityReason { get; set; }

        public IEnumerable<SelectListItem> Wards { get; set; }
    }
}