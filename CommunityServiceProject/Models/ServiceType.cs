using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class ServiceType
    {
        [Key]
        public int ServiceTypeID { get; set; }

        [Required]
        [StringLength(30)]
        public string ServiceCode { get; set; }

        [Required]
        [StringLength(150)]
        public string ServiceName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public bool IsChargeable { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public virtual ICollection<FeeSchedule> FeeSchedules { get; set; }

        public virtual ICollection<MunicipalServiceRequest> MunicipalServiceRequests { get; set; }

        public ServiceType()
        {
            FeeSchedules = new HashSet<FeeSchedule>();
            MunicipalServiceRequests = new HashSet<MunicipalServiceRequest>();
        }
    }
}