using CommunityServiceProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CommunityServiceProject.Models
{
    public class Community : DbContext
    {
        public Community() : base("Community")
        {
        }

        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}