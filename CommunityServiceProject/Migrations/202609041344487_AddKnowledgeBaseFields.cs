namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKnowledgeBaseFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MaintenanceKnowledgeBases", "CreatedByTechnicianID", c => c.Int(nullable: false));
            AddColumn("dbo.MaintenanceKnowledgeBases", "ProblemDescription", c => c.String(nullable: false, maxLength: 2000));
            AddColumn("dbo.MaintenanceKnowledgeBases", "RecommendedSolution", c => c.String(nullable: false, maxLength: 2000));
            AddColumn("dbo.MaintenanceKnowledgeBases", "LessonsLearned", c => c.String(maxLength: 2000));
            AddColumn("dbo.MaintenanceKnowledgeBases", "IsActive", c => c.Boolean(nullable: false));
            CreateIndex("dbo.MaintenanceKnowledgeBases", "CreatedByTechnicianID");
            AddForeignKey("dbo.MaintenanceKnowledgeBases", "CreatedByTechnicianID", "dbo.Technicians", "TechnicianID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MaintenanceKnowledgeBases", "CreatedByTechnicianID", "dbo.Technicians");
            DropIndex("dbo.MaintenanceKnowledgeBases", new[] { "CreatedByTechnicianID" });
            DropColumn("dbo.MaintenanceKnowledgeBases", "IsActive");
            DropColumn("dbo.MaintenanceKnowledgeBases", "LessonsLearned");
            DropColumn("dbo.MaintenanceKnowledgeBases", "RecommendedSolution");
            DropColumn("dbo.MaintenanceKnowledgeBases", "ProblemDescription");
            DropColumn("dbo.MaintenanceKnowledgeBases", "CreatedByTechnicianID");
        }
    }
}
