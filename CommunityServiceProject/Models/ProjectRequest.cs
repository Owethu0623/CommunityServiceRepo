using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class ProjectRequest
    {
        [Key]
        public int ProjectRequestID { get; set; }

        [Index("IX_ProjectRequest_Project_Request", 1, IsUnique = true)]
        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public virtual MunicipalProject Project { get; set; }

        [Index("IX_ProjectRequest_Project_Request", 2, IsUnique = true)]
        public int RequestID { get; set; }

        [ForeignKey("RequestID")]
        public virtual Request Request { get; set; }


        [StringLength(100)]
        [Display(Name = "Relationship Type")]
        public string RelationshipType { get; set; }


        [Required]
        [Display(Name = "Date Linked")]
        public DateTime DateLinked { get; set; }


        [Required]
        [Display(Name = "Linked By")]
        public int LinkedByAdministratorID { get; set; }

        [ForeignKey("LinkedByAdministratorID")]
        public virtual Administrator LinkedByAdministrator { get; set; }


        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}