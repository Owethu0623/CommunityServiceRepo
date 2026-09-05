using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class RequestSkill
    {
        [Key]
        public int RequestSkillID { get; set; }

        [Required]
        [Index("IX_RequestSkill_Request_Skill", 1, IsUnique = true)]
        public int RequestID { get; set; }

        public virtual Request Request { get; set; }

        [Required]
        [Index("IX_RequestSkill_Request_Skill", 2, IsUnique = true)]
        public int SkillID { get; set; }

        public virtual Skill Skill { get; set; }
    }
}