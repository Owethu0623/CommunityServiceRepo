namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<CommunityServiceProject.Models.Community>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "CommunityServiceProject.Models.Community";
        }

      
               protected override void Seed(CommunityServiceProject.Models.Community context)
        {
            // Only create the default Administrator if one does not already exist
            if (!context.Administrators.Any(a =>
                a.EmailAddress == "admin@municipality.co.za"))
            {
                var administrator = new CommunityServiceProject.Models.Administrator
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailAddress = "admin@municipality.co.za",
                    PhoneNumber = "0123456789",
                    Password = "Admin123",
                    AccountStatus = CommunityServiceProject.Models.AccountStatus.Active
                };

                context.Administrators.Add(administrator);
                context.SaveChanges();
            }


            // ===========================================================
            // DEFAULT TECHNICIAN SKILLS
            // ===========================================================

            context.Skills.AddOrUpdate(
                s => s.SkillName,

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Plumbing",
                    Description = "Skills related to water supply, leaks, drainage and sewer maintenance."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Electrical",
                    Description = "Skills related to electrical systems, wiring and street lighting."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Road Maintenance",
                    Description = "Skills related to potholes, damaged roads and road surface repairs."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Waste Management",
                    Description = "Skills related to municipal waste, illegal dumping and overflowing bins."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Traffic Sign Maintenance",
                    Description = "Skills related to the repair and maintenance of damaged traffic signs."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Sidewalk Maintenance",
                    Description = "Skills related to damaged sidewalks and pedestrian pathways."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "General Maintenance",
                    Description = "General municipal maintenance and repair skills."
                }
            );
        }


    }
}