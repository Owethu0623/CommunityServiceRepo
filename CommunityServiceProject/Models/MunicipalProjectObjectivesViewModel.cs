using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectObjectivesViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public List<ProjectObjectiveItemViewModel> Objectives { get; set; }

        public MunicipalProjectObjectivesViewModel()
        {
            Objectives = new List<ProjectObjectiveItemViewModel>();
        }
    }

    public class ProjectObjectiveItemViewModel
    {
        public int ProjectObjectiveID { get; set; }

        [Required(ErrorMessage = "Please enter an objective title.")]
        [StringLength(
            200,
            MinimumLength = 3,
            ErrorMessage = "Objective title must be between 3 and 200 characters."
        )]
        [Display(Name = "Objective Title")]
        public string ObjectiveTitle { get; set; }

        [Required(ErrorMessage = "Please enter an objective description.")]
        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "Objective description must be between 10 and 2000 characters."
        )]
        [Display(Name = "Objective Description")]
        public string ObjectiveDescription { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public MunicipalProjectPriority Priority { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Target Date")]
        public DateTime? TargetDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public ProjectObjectiveStatus Status { get; set; }
    }
}