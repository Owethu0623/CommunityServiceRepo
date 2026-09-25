using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianOpportunityCreateViewModel
    {
        [Required(ErrorMessage = "Please enter the opportunity title.")]
        [StringLength(
            200,
            MinimumLength = 5,
            ErrorMessage = "The opportunity title must be between 5 and 200 characters."
        )]
        [Display(Name = "Opportunity Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please enter the opportunity description.")]
        [StringLength(
            4000,
            MinimumLength = 20,
            ErrorMessage = "The description must be between 20 and 4000 characters."
        )]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter the responsibilities.")]
        [StringLength(
            4000,
            MinimumLength = 20,
            ErrorMessage = "Responsibilities must be between 20 and 4000 characters."
        )]
        [Display(Name = "Responsibilities")]
        public string Responsibilities { get; set; }

        [Required(ErrorMessage = "Please enter the requirements.")]
        [StringLength(
            4000,
            MinimumLength = 20,
            ErrorMessage = "Requirements must be between 20 and 4000 characters."
        )]
        [Display(Name = "Requirements")]
        public string Requirements { get; set; }

        [Required(ErrorMessage = "Please enter the required qualifications.")]
        [StringLength(
            2000,
            MinimumLength = 10,
            ErrorMessage = "Required qualifications must be between 10 and 2000 characters."
        )]
        [Display(Name = "Required Qualifications")]
        public string RequiredQualifications { get; set; }

        [StringLength(
            2000,
            ErrorMessage = "Required experience cannot exceed 2000 characters."
        )]
        [Display(Name = "Required Experience")]
        public string RequiredExperience { get; set; }

        [StringLength(
            2000,
            ErrorMessage = "Application instructions cannot exceed 2000 characters."
        )]
        [Display(Name = "Application Instructions")]
        public string ApplicationInstructions { get; set; }

        [Required(ErrorMessage = "Please select an employment type.")]
        [StringLength(100)]
        [Display(Name = "Employment Type")]
        public string EmploymentType { get; set; }

        [Range(
            1,
            999,
            ErrorMessage = "Number of positions must be between 1 and 999."
        )]
        [Display(Name = "Number of Positions")]
        public int? NumberOfPositions { get; set; }

        [Required(ErrorMessage = "Please enter the application start date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Application Start Date")]
        public DateTime ApplicationStartDate { get; set; }

        [Required(ErrorMessage = "Please enter the application deadline.")]
        [DataType(DataType.Date)]
        [Display(Name = "Application Deadline")]
        public DateTime ApplicationDeadline { get; set; }
    }
}