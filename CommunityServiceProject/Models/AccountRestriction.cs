using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class AccountRestriction
    {
        [Key]
        public int RestrictionID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        public virtual Citizen Citizen { get; set; }

        [Required]
        public int AdministratorID { get; set; }

        public virtual Administrator Administrator { get; set; }

        [Required]
        [StringLength(50)]
        public string RestrictionType { get; set; }

        [Required]
        [StringLength(2000)]
        public string Reason { get; set; }

        [Required]
        public DateTime DateStarted { get; set; }

        public DateTime? DateEnded { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}

