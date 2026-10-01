namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApplicationDocument : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationDocuments",
                c => new
                    {
                        ApplicationDocumentID = c.Int(nullable: false, identity: true),
                        ApplicationID = c.Int(nullable: false),
                        DocumentType = c.String(nullable: false, maxLength: 100),
                        DocumentTitle = c.String(nullable: false, maxLength: 200),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FilePath = c.String(nullable: false, maxLength: 500),
                        ContentType = c.String(nullable: false, maxLength: 100),
                        FileSize = c.Long(nullable: false),
                        DateSubmitted = c.DateTime(nullable: false),
                        VerificationStatus = c.Int(nullable: false),
                        VerificationComments = c.String(maxLength: 1000),
                        VerifiedByAdministratorID = c.Int(),
                        VerificationDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ApplicationDocumentID)
                .ForeignKey("dbo.TechnicianApplications", t => t.ApplicationID, cascadeDelete: true)
                .ForeignKey("dbo.Administrators", t => t.VerifiedByAdministratorID)
                .Index(t => t.ApplicationID)
                .Index(t => t.VerifiedByAdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationDocuments", "VerifiedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ApplicationDocuments", "ApplicationID", "dbo.TechnicianApplications");
            DropIndex("dbo.ApplicationDocuments", new[] { "VerifiedByAdministratorID" });
            DropIndex("dbo.ApplicationDocuments", new[] { "ApplicationID" });
            DropTable("dbo.ApplicationDocuments");
        }
    }
}
