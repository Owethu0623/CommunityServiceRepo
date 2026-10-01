
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class FinanceOfficer
    {
        [Key]
        public int FinanceOfficerID { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(
            @"^[A-Za-z]+([ '-][A-Za-z]+)*$",
            ErrorMessage = "First name can contain letters, spaces, hyphens and apostrophes only."
        )]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(
            @"^[A-Za-z]+([ '-][A-Za-z]+)*$",
            ErrorMessage = "Last name can contain letters, spaces, hyphens and apostrophes only."
        )]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [Index(IsUnique = true)]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must be at least 8 characters and contain an uppercase letter, a lowercase letter, and a number."
        )]
        public string Password { get; set; }

        [Required]
        public AccountStatus AccountStatus { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public void Create()
        {
            DateCreated = DateTime.Now;
            AccountStatus = AccountStatus.Active;
        }

        public virtual ICollection<FinancialAudit> FinancialAudits { get; set; }

        public FinanceOfficer()
        {
            FinancialAudits = new HashSet<FinancialAudit>();
        }
    }
}
