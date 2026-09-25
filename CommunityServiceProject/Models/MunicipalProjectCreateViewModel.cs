using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectCreateViewModel
    {
        [Required(ErrorMessage = "Please provide the project name.")]
        [StringLength(
            200,
            MinimumLength = 3,
            ErrorMessage = "Project name must be between 3 and 200 characters."
        )]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Please select the project type.")]
        [Display(Name = "Project Type")]
        public string ProjectType { get; set; }

        [StringLength(
            2000,
            ErrorMessage = "Description cannot exceed 2000 characters."
        )]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please provide the project location.")]
        [StringLength(
            500,
            MinimumLength = 3,
            ErrorMessage = "Project location must be between 3 and 500 characters."
        )]
        [Display(Name = "Project Location")]
        public string ProjectLocation { get; set; }

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

        [Required(ErrorMessage = "Please select a municipal ward.")]
        [Display(Name = "Ward")]
        public int? WardID { get; set; }

        [Required(ErrorMessage = "Please provide the project start date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Expected Completion Date")]
        public DateTime? ExpectedCompletionDate { get; set; }

        [Required(ErrorMessage = "Please select the project priority.")]
        [Display(Name = "Priority")]
        public MunicipalProjectPriority? Priority { get; set; }

        [Range(
            0,
            9999999999999999.99,
            ErrorMessage = "Estimated budget must be zero or greater."
        )]
        [Display(Name = "Estimated Budget")]
        public decimal? EstimatedBudget { get; set; }

        [Required(ErrorMessage = "Please select the responsible administrator.")]
        [Display(Name = "Responsible Administrator")]
        public int? ResponsibleAdministratorID { get; set; }

        public List<SelectListItem> ProjectTypeOptions { get; set; }

        public List<SelectListItem> WardOptions { get; set; }

        public List<SelectListItem> PriorityOptions { get; set; }

        public List<SelectListItem> AdministratorOptions { get; set; }

        public MunicipalProjectCreateViewModel()
        {
            ProjectTypeOptions = new List<SelectListItem>();
            WardOptions = new List<SelectListItem>();
            PriorityOptions = new List<SelectListItem>();
            AdministratorOptions = new List<SelectListItem>();
        }
    }
}