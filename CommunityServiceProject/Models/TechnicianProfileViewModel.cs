using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianProfileViewModel
    {
        public Technician Technician { get; set; }

        public List<Skill> Skills { get; set; }

        public int CompletedRequestCount { get; set; }

        public int ActiveRequestCount { get; set; }

        public TechnicianProfileViewModel()
        {
            Skills = new List<Skill>();
        }
    }
}