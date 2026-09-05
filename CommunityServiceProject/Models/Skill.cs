using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class Skill
    {
        [Key]
        public int SkillID { get; set; }

        [Required]
        [StringLength(100)]
        public string SkillName { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        // Relationship with technicians
        public virtual ICollection<TechnicianSkill> TechnicianSkills { get; set; }

        // Constructor
        public Skill()
        {
            TechnicianSkills = new List<TechnicianSkill>();
        }
    }
}