using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Technician
    {
        [Key]
        public int TechnicianID { get; set; }


        // ===========================================================
        // PERSONAL INFORMATION
        // ===========================================================

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string LastName { get; set; }


        // ===========================================================
        // CONTACT INFORMATION
        // ===========================================================

        [Index("IX_Technician_EmailAddress", IsUnique = true)]
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100)]
        public string EmailAddress { get; set; }


        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }


        // ===========================================================
        // LOGIN INFORMATION
        // ===========================================================

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
        public string Password { get; set; }


        // ===========================================================
        // CONFIRM PASSWORD
        // ===========================================================
        // This property is ONLY used when creating/editing the
        // technician account. It is NOT stored in the database.

        [NotMapped]
        [Required(ErrorMessage = "Please confirm the password.")]
        [DataType(DataType.Password)]
        [Compare(
            "Password",
            ErrorMessage = "Passwords do not match."
        )]
        public string ConfirmPassword { get; set; }


        // ===========================================================
        // ACCOUNT STATUS
        // ===========================================================

        [Required]
        public AccountStatus AccountStatus { get; set; }


        // ===========================================================
        // TECHNICIAN RELATIONSHIPS
        // ===========================================================

        public virtual ICollection<Request> Requests { get; set; }

        public virtual ICollection<TechnicianAssignment> TechnicianAssignments { get; set; }

        public virtual ICollection<TechnicianSkill> TechnicianSkills { get; set; }


        // ===========================================================
        // CONSTRUCTOR
        // ===========================================================

        public Technician()
        {
            Requests = new List<Request>();
            TechnicianAssignments = new List<TechnicianAssignment>();
            TechnicianSkills = new List<TechnicianSkill>();
        }
    }
}