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
        }
    }
}