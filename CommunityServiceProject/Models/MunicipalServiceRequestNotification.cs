
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class MunicipalServiceRequestNotification
    {
        [Key]
        public int MunicipalServiceRequestNotificationID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [ForeignKey("CitizenID")]
        public virtual Citizen Citizen { get; set; }

        [Required]
        public int MunicipalServiceRequestID { get; set; }

        [ForeignKey("MunicipalServiceRequestID")]
        public virtual MunicipalServiceRequest MunicipalServiceRequest { get; set; }

        [Required]
        public MunicipalServiceRequestNotificationType NotificationType { get; set; }

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

