using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianOnboardingViewModel
    {
        // =========================================================
        // APPLICATION / APPLICANT INFORMATION
        // =========================================================

        public int ApplicationID { get; set; }

        [Display(Name = "Application Reference")]
        public string ApplicationReference { get; set; }

        [Display(Name = "Applicant")]
        public string ApplicantName { get; set; }

        [Display(Name = "Personal Email")]
        public string PersonalEmail { get; set; }

        [Display(Name = "Phone Number")]
        public string ApplicantPhoneNumber { get; set; }

        [Display(Name = "Application Date")]
        [DataType(DataType.DateTime)]
        public DateTime ApplicationDate { get; set; }


        // =========================================================
        // OPPORTUNITY INFORMATION
        // =========================================================

        [Display(Name = "Technician Opportunity")]
        public string OpportunityTitle { get; set; }

        [Display(Name = "Opportunity Code")]
        public string OpportunityCode { get; set; }

        [Display(Name = "Employment Type")]
        public string EmploymentType { get; set; }


        // =========================================================
        // RECRUITMENT EVIDENCE
        // =========================================================

        [Display(Name = "Screening Result")]
        public string ScreeningResult { get; set; }

        [Display(Name = "Documents Verified")]
        public int VerifiedDocumentCount { get; set; }

        [Display(Name = "Documents Requiring Attention")]
        public int UnverifiedDocumentCount { get; set; }

        [Display(Name = "Assessment Result")]
        public string AssessmentResult { get; set; }

        [Display(Name = "Assessment Score")]
        public decimal? AssessmentScore { get; set; }

        [Display(Name = "Interview Outcome")]
        public string InterviewOutcome { get; set; }

        [Display(Name = "Selection Comments")]
        public string SelectionComments { get; set; }

        [Display(Name = "Final Verification")]
        public string FinalVerificationResult { get; set; }

        [Display(Name = "Final Verification Comments")]
        public string FinalVerificationComments { get; set; }


        // =========================================================
        // TECHNICIAN ACCOUNT
        // =========================================================

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(
            50,
            ErrorMessage = "First name cannot exceed 50 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ '-][A-Za-z]+)*$",
            ErrorMessage = "First name may contain letters, spaces, hyphens and apostrophes only."
        )]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(
            50,
            ErrorMessage = "Last name cannot exceed 50 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ '-][A-Za-z]+)*$",
            ErrorMessage = "Last name may contain letters, spaces, hyphens and apostrophes only."
        )]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Municipal email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(
            100,
            ErrorMessage = "Email address cannot exceed 100 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z0-9._%+-]+@municipality\.co\.za$",
            ErrorMessage = "Municipal email address must use the @municipality.co.za domain."
        )]
        [Display(Name = "Municipal Email Address")]
        public string MunicipalEmail { get; set; }


        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^0\d{9}$",
            ErrorMessage = "Phone number must be exactly 10 digits and start with 0."
        )]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }


        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long."
        )]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, and a number."
        )]
        [Display(Name = "Initial Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm the password.")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare(
            "Password",
            ErrorMessage = "Passwords do not match."
        )]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }


        // =========================================================
        // PRIMARY TECHNICIAN SKILL
        // =========================================================

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a primary technician skill."
        )]
        [Display(Name = "Primary Skill")]
        public int SelectedSkillID { get; set; }

        public List<SelectListItem> AvailableSkills { get; set; }


        // =========================================================
        // CONFIRMATION
        // =========================================================

        [Range(
            typeof(bool),
            "true",
            "true",
            ErrorMessage = "Please confirm that the onboarding information has been reviewed."
        )]
        [Display(Name = "Confirm Onboarding")]
        public bool ConfirmOnboarding { get; set; }


        // =========================================================
        // WORKFLOW STATE
        // =========================================================

        public TechnicianApplicationStatus ApplicationStatus { get; set; }

        public bool CanOnboard
        {
            get
            {
                return ApplicationStatus ==
                       TechnicianApplicationStatus.ApprovedForOnboarding;
            }
        }


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public TechnicianOnboardingViewModel()
        {
            AvailableSkills = new List<SelectListItem>();
        }
    }
}