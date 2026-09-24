using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class EditTechnicianViewModel
    {
        public int TechnicianID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        [RegularExpression(
            @"^[A-Za-zÀ-ÿ]+([ '-][A-Za-zÀ-ÿ]+)*$",
            ErrorMessage = "First name may contain letters, spaces, hyphens and apostrophes only.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        [RegularExpression(
            @"^[A-Za-zÀ-ÿ]+([ '-][A-Za-zÀ-ÿ]+)*$",
            ErrorMessage = "Last name may contain letters, spaces, hyphens and apostrophes only.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters.")]
        [RegularExpression(
            @"^[A-Za-z0-9._%+-]+@municipality\.co\.za$",
            ErrorMessage = "Technician email must use the @municipality.co.za domain.")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^0\d{9}$",
            ErrorMessage = "Phone number must be exactly 10 digits and start with 0.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public List<Skill> AssignedSkills { get; set; }

        public List<Skill> AvailableSkills { get; set; }

        public EditTechnicianViewModel()
        {
            AssignedSkills = new List<Skill>();
            AvailableSkills = new List<Skill>();
        }
    }
}