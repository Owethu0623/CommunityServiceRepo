using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class ComplianceRecord
    {
        [Key]
        public int ComplianceID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        public virtual Citizen Citizen { get; set; }

        [Required]
        public int ConfirmedViolationCount { get; set; }

        [Required]
        public ComplianceStatus ComplianceStatus { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

        public virtual ICollection<Violation> Violations { get; set; }

        public ComplianceRecord()
        {
            Violations = new List<Violation>();
        }
    }
}

