using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class Warning
    {
        [Key]
        public int WarningID { get; set; }

        [Required]
        public int ViolationID { get; set; }

        public virtual Violation Violation { get; set; }

        [Required]
        public int AdministratorID { get; set; }

        public virtual Administrator Administrator { get; set; }

        [Required]
        [StringLength(2000)]
        public string WarningReason { get; set; }

        [Required]
        public DateTime DateIssued { get; set; }
    }
}

