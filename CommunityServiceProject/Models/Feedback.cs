using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        [Required]
        [Index("IX_Feedback_RequestID", IsUnique = true)]
        public int RequestID { get; set; }

        public virtual Request Request { get; set; }

        [Required]
        public int CitizenID { get; set; }

        public virtual Citizen Citizen { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(2000)]
        public string Comment { get; set; }

        [Required]
        public FeedbackResolutionStatus ResolutionStatus { get; set; }

        [Required]
        public DateTime DateSubmitted { get; set; }

        public bool Submit()
        {
            DateSubmitted = DateTime.Now;
            return true;
        }

        public bool Update()
        {
            return true;
        }
    }
}