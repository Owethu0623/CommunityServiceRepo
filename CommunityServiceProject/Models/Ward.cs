using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class Ward
    {
        [Key]
        public int WardID { get; set; }

        [Required]
        [StringLength(100)]
        public string WardName { get; set; }

        [Required]
        [StringLength(20)]
        public string WardNumber { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        public virtual ICollection<Request> Requests { get; set; }

        public Ward()
        {
            Requests = new List<Request>();
        }
    }
}
