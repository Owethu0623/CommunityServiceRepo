using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class HROfficer
    {
        [Key]
        public int HROfficerID { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [Index(IsUnique = true)]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
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
    }
}
