using CommunityServiceProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CommunityServiceProject.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        // Navigation property
        public virtual ICollection<Request> Requests { get; set; }

        public Category()
        {
            Requests = new List<Request>();
        }

        // Business method
        public int GetTotalRequests()
        {
            return Requests.Count;
        }
    }

}