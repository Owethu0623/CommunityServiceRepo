using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CommunityServiceProject.Models
{
    public class Administrator
    {
        [Key]
        public int user { get; set; }
    }
}