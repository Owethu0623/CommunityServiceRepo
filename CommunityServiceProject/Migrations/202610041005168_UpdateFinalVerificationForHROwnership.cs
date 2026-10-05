using System.Data.Entity.Migrations;

public partial class UpdateFinalVerificationForHROwnership : DbMigration
{
    public override void Up()
    {
        AlterColumn(
            "dbo.TechnicianApplicationFinalVerifications",
            "VerifiedByAdministratorID",
            c => c.Int());

        AddColumn(
            "dbo.TechnicianApplicationFinalVerifications",
            "VerifiedByHROfficerID",
            c => c.Int());

        CreateIndex(
            "dbo.TechnicianApplicationFinalVerifications",
            "VerifiedByHROfficerID");

        AddForeignKey(
            "dbo.TechnicianApplicationFinalVerifications",
            "VerifiedByHROfficerID",
            "dbo.HROfficers",
            "HROfficerID");
    }

    public override void Down()
    {
        DropForeignKey(
            "dbo.TechnicianApplicationFinalVerifications",
            "VerifiedByHROfficerID",
            "dbo.HROfficers");

        DropIndex(
            "dbo.TechnicianApplicationFinalVerifications",
            new[] { "VerifiedByHROfficerID" });

        DropColumn(
            "dbo.TechnicianApplicationFinalVerifications",
            "VerifiedByHROfficerID");

        AlterColumn(
            "dbo.TechnicianApplicationFinalVerifications",
            "VerifiedByAdministratorID",
            c => c.Int(nullable: false));
    }
}