namespace CommunityServiceProject.Migrations
{
    using CommunityServiceProject.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<CommunityServiceProject.Models.Community>
    {
        public Configuration()
        {
            // Enable automatic migrations so pending model changes (like the new HROfficer entity)
            // can be applied without creating an explicit code-based migration.
            // This is a short-term convenience to unblock development; consider
            // reverting to explicit migrations for production.
            AutomaticMigrationsEnabled = true;
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

            context.FinanceOfficers.AddOrUpdate(
f => f.EmailAddress,
new FinanceOfficer
{
FirstName = "Municipal",
LastName = "Finance Officer",
EmailAddress = "finance@municipality.co.za",
Password = "Finance123",
AccountStatus = AccountStatus.Active,
DateCreated = DateTime.Now
}
);

            // Seed a default HR Officer for development/testing so RBAC can be exercised
            context.HROfficers.AddOrUpdate(
                h => h.EmailAddress,
                new HROfficer
                {
                    FirstName = "Municipal",
                    LastName = "HR Officer",
                    EmailAddress = "hr@municipality.co.za",
                    Password = "HR123",
                    AccountStatus = AccountStatus.Active,
                    DateCreated = DateTime.Now
                }
            );


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