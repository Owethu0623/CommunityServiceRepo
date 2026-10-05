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

        // Historical administrator (nullable to support HR selection ownership)
        public int? SelectedByAdministratorID { get; set; }

        [ForeignKey("SelectedByAdministratorID")]
        public virtual Administrator SelectedByAdministrator { get; set; }

        // New: preserve existing Administrator historical owner but allow HR officer
        // to be recorded as the selector going forward. Nullable to avoid
        // breaking existing data.
        public int? SelectedByHROfficerID { get; set; }

        [ForeignKey("SelectedByHROfficerID")]
        public virtual HROfficer SelectedByHROfficer { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Selection Date")]
        public DateTime SelectionDate { get; set; }
    }
}