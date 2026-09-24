using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [ForeignKey("CitizenID")]
        public virtual Citizen Citizen { get; set; }

        public int? RequestID { get; set; }

        [ForeignKey("RequestID")]
        public virtual Request Request { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public bool IsRead { get; set; }
    }
}