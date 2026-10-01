using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class TechnicianApplicationSelection
    {
        [Key]
        public int SelectionID { get; set; }

        [Index("IX_TechnicianApplicationSelection_Application", IsUnique = true)]
        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual TechnicianApplication Application { get; set; }

        [StringLength(3000)]
        [Display(Name = "Selection Comments")]
        public string Comments { get; set; }

        [Required]
        public int SelectedByAdministratorID { get; set; }

        [ForeignKey("SelectedByAdministratorID")]
        public virtual Administrator SelectedByAdministrator { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Selection Date")]
        public DateTime SelectionDate { get; set; }
    }
}