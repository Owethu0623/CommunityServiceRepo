using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class TechnicianSkill
    {
        [Key]
        public int TechnicianSkillID { get; set; }

        [Required]
        [Index("IX_TechnicianSkill_Technician_Skill", 1, IsUnique = true)]
        public int TechnicianID { get; set; }

        public virtual Technician Technician { get; set; }

        [Required]
        [Index("IX_TechnicianSkill_Technician_Skill", 2, IsUnique = true)]
        public int SkillID { get; set; }

        public virtual Skill Skill { get; set; }
    }
}