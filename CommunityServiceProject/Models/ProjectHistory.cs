using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class ProjectHistory
    {
        [Key]
        public int ProjectHistoryID { get; set; }


        [Required]
        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public virtual MunicipalProject Project { get; set; }


        [Required]
        [StringLength(50)]
        [Display(Name = "Action Type")]
        public string ActionType { get; set; }


        [Required]
        [StringLength(2000)]
        [Display(Name = "Description")]
        public string Description { get; set; }


        [StringLength(50)]
        [Display(Name = "Previous Status")]
        public string PreviousStatus { get; set; }


        [StringLength(50)]
        [Display(Name = "New Status")]
        public string NewStatus { get; set; }


        [Required]
        [Display(Name = "Action Date")]
        public DateTime ActionDate { get; set; }


        [Required]
        [Display(Name = "Performed By")]
        public int PerformedByAdministratorID { get; set; }

        [ForeignKey("PerformedByAdministratorID")]
        public virtual Administrator PerformedByAdministrator { get; set; }
    }
}