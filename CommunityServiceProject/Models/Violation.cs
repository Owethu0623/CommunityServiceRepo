using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class Violation
    {
        [Key]
        public int ViolationID { get; set; }

        [Required]
        public int ComplianceID { get; set; }

        public virtual ComplianceRecord ComplianceRecord { get; set; }

        public int? RequestID { get; set; }

        public virtual Request Request { get; set; }

        [Required]
        public int AdministratorID { get; set; }

        public virtual Administrator Administrator { get; set; }

        [Required]
        [StringLength(100)]
        public string ViolationType { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public DateTime DateConfirmed { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public virtual ICollection<Warning> Warnings { get; set; }

        public Violation()
        {
            Warnings = new List<Warning>();
        }
    }
}

