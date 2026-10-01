using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class ApplicationInterview
    {
        [Key]
        public int InterviewID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual TechnicianApplication Application { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Interview Date")]
        public DateTime InterviewDate { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Location")]
        public string Location { get; set; }

        [StringLength(
    3000,
    ErrorMessage = "Interview instructions cannot exceed 3000 characters."
)]
        [Display(Name = "Interview Instructions")]
        public string Instructions { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Interview Method")]
        public string InterviewMethod { get; set; }

        [Required]
        [Display(Name = "Interview Status")]
        public ApplicationInterviewStatus Status { get; set; }

        [StringLength(2000)]
        [Display(Name = "Interview Outcome")]
        public string Outcome { get; set; }

        [StringLength(3000)]
        [Display(Name = "Interview Comments")]
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