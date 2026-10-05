using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class HROfficerNotification
    {
        [Key]
        public int HROfficerNotificationID { get; set; }

        [Required]
        public int HROfficerID { get; set; }

        [ForeignKey("HROfficerID")]
        public virtual HROfficer HROfficer { get; set; }

        public int? ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual TechnicianApplication Application { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }
    }
}
