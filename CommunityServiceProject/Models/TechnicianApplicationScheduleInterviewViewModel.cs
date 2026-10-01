using System;
using System.ComponentModel.DataAnnotations;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianApplicationScheduleInterviewViewModel
    {
        public int ApplicationID { get; set; }

        public string ApplicationReference { get; set; }

        public string ApplicantName { get; set; }

        public string ApplicantEmail { get; set; }

        public string OpportunityCode { get; set; }

        public string OpportunityTitle { get; set; }

        public DateTime ApplicationDate { get; set; }

        public TechnicianApplicationStatus ApplicationStatus { get; set; }

        [Required(ErrorMessage = "Please select an interview method.")]
        [StringLength(100)]
        [Display(Name = "Interview Method")]
        public string InterviewMethod { get; set; }

        [Required(ErrorMessage = "Please select the interview date and time.")]
        [Display(Name = "Interview Date and Time")]
        public DateTime InterviewDate { get; set; }

        [Required(ErrorMessage = "Please enter the interview location.")]
        [StringLength(
            300,
            MinimumLength = 3,
            ErrorMessage = "The interview location must be between 3 and 300 characters."
        )]
        [Display(Name = "Interview Location")]
        public string Location { get; set; }

        [StringLength(
            3000,
            ErrorMessage = "Interview instructions cannot exceed 3000 characters."
        )]
        [Display(Name = "Interview Instructions")]
        public string Instructions { get; set; }
    }
}