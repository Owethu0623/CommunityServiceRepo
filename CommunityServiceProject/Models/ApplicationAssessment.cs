using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class ApplicationAssessment
    {
        [Key]
        public int AssessmentID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual TechnicianApplication Application { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Assessment Type")]
        public string AssessmentType { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Assessment Date")]
        public DateTime AssessmentDate { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Assessment Location")]
        public string Location { get; set; }

        [StringLength(3000)]
        [Display(Name = "Instructions")]
        public string Instructions { get; set; }

        [Required]
        [Display(Name = "Assessment Status")]
        public ApplicationAssessmentStatus Status { get; set; }

        [StringLength(2000)]
        [Display(Name = "Result")]
        public string Result { get; set; }

        [Range(0, 100)]
        [Display(Name = "Score")]
        public decimal? Score { get; set; }

        [StringLength(3000)]
        [Display(Name = "Comments")]
        public string Comments { get; set; }

        [Required]
        public int RecordedByAdministratorID { get; set; }

        [ForeignKey("RecordedByAdministratorID")]
        public virtual Administrator RecordedByAdministrator { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateRecorded { get; set; }
    }
}