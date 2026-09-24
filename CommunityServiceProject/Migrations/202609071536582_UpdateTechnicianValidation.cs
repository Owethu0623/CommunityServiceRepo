namespace CommunityServiceProject.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class UpdateTechnicianValidation : DbMigration
    {
        public override void Up()
        {
            // Keep technician email unique.
            DropIndex("dbo.Technicians", "IX_Technician_EmailAddress");

            CreateIndex(
                "dbo.Technicians",
                "EmailAddress",
                unique: true,
                name: "IX_Technician_EmailAddress");

            // Keep the existing phone-number database length.
            AlterColumn(
                "dbo.Technicians",
                "PhoneNumber",
                c => c.String(nullable: false, maxLength: 20));
        }

        public override void Down()
        {
            DropIndex(
                "dbo.Technicians",
                "IX_Technician_EmailAddress");

            AlterColumn(
                "dbo.Technicians",
                "PhoneNumber",
                c => c.String(nullable: false, maxLength: 20));

            CreateIndex(
                "dbo.Technicians",
                "EmailAddress",
                unique: true,
                name: "IX_Technician_EmailAddress");
        }
    }
}