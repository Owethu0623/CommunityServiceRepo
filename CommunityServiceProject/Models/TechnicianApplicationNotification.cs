
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class TechnicianApplicationNotification
    {
        [Key]
        public int TechnicianApplicationNotificationID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual TechnicianApplication Application { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [ForeignKey("CitizenID")]
        public virtual Citizen Citizen { get; set; }

        [Required]
        [Display(Name = "Notification Type")]
        public TechnicianApplicationNotificationType NotificationType { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }

        [Required]
        [Display(Name = "Read")]
        public bool IsRead { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Read Date")]
        public DateTime? ReadDate { get; set; }
    }
}

