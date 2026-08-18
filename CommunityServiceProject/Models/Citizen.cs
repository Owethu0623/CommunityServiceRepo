using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Citizen
    {
        [Key]
        public int CitizenID { get; set; }
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z]+([ '-][A-Za-z]+)*$",
            ErrorMessage = "First name can contain letters, spaces, hyphens and apostrophes only.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z]+([ '-][A-Za-z]+)*$",
            ErrorMessage = "Last name can contain letters, spaces, hyphens and apostrophes only.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [RegularExpression(@"^0\d{9}$",
            ErrorMessage = "Enter a valid 10-digit South African phone number.")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(
    @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
    ErrorMessage = "Password must be at least 8 characters and contain an uppercase letter, a lowercase letter, and a number."
)]
        public string Password { get; set; }

        [NotMapped]
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(200)]
        public string ResidentialAddress { get; set; }

        public DateTime DateRegistered { get; set; }

        public AccountStatus AccountStatus { get; set; }

        // Business methods
        public void Register()
        {
            DateRegistered = DateTime.Now;
            AccountStatus = AccountStatus.Active;
        }

        public void Login()
        {
        }

        public void Logout()
        {
        }

        public void ForgotPassword()
        {
        }
    }
}