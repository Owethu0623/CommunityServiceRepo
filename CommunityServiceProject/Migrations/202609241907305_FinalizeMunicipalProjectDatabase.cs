namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FinalizeMunicipalProjectDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectEvidences",
                c => new
                    {
                        ProjectEvidenceID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        EvidenceTitle = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 2000),
                        EvidenceType = c.String(nullable: false, maxLength: 50),
                        FileName = c.String(nullable: false, maxLength: 500),
                        FilePath = c.String(nullable: false, maxLength: 1000),
                        ContentType = c.String(maxLength: 150),
                        FileSize = c.Long(),
                        DateRecorded = c.DateTime(nullable: false),
                        RecordedByAdministratorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProjectEvidenceID)
                .ForeignKey("dbo.MunicipalProjects", t => t.ProjectID)
                .ForeignKey("dbo.Administrators", t => t.RecordedByAdministratorID)
                .Index(t => t.ProjectID)
                .Index(t => t.RecordedByAdministratorID);
            
            CreateTable(
                "dbo.ProjectHistories",
                c => new
                    {
                        ProjectHistoryID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        ActionType = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 2000),
                        PreviousStatus = c.String(maxLength: 50),
                        NewStatus = c.String(maxLength: 50),
                        ActionDate = c.DateTime(nullable: false),
                        PerformedByAdministratorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProjectHistoryID)
                .ForeignKey("dbo.Administrators", t => t.PerformedByAdministratorID)
                .ForeignKey("dbo.MunicipalProjects", t => t.ProjectID)
                .Index(t => t.ProjectID)
                .Index(t => t.PerformedByAdministratorID);
            
            CreateTable(
                "dbo.ProjectMilestones",
                c => new
                    {
                        ProjectMilestoneID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        MilestoneName = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 2000),
                        PlannedStartDate = c.DateTime(nullable: false),
                        PlannedCompletionDate = c.DateTime(nullable: false),
                        ActualCompletionDate = c.DateTime(),
                        Status = c.Int(nullable: false),
                        ProgressPercentage = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        SequenceNumber = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        CreatedByAdministratorID = c.Int(nullable: false),
                        LastUpdatedDate = c.DateTime(),
                        LastUpdatedByAdministratorID = c.Int(),
                    })
                .PrimaryKey(t => t.ProjectMilestoneID)
                .ForeignKey("dbo.Administrators", t => t.CreatedByAdministratorID)
                .ForeignKey("dbo.Administrators", t => t.LastUpdatedByAdministratorID)
                .ForeignKey("dbo.MunicipalProjects", t => t.ProjectID)
                .Index(t => t.ProjectID)
                .Index(t => t.CreatedByAdministratorID)
                .Index(t => t.LastUpdatedByAdministratorID);
            
            CreateTable(
                "dbo.ProjectObjectives",
                c => new
                    {
                        ProjectObjectiveID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        ObjectiveTitle = c.String(nullable: false, maxLength: 200),
                        ObjectiveDescription = c.String(nullable: false, maxLength: 2000),
                        Priority = c.Int(nullable: false),
                        TargetDate = c.DateTime(),
                        Status = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        CreatedByAdministratorID = c.Int(nullable: false),
                        LastUpdatedDate = c.DateTime(),
                        LastUpdatedByAdministratorID = c.Int(),
                    })
                .PrimaryKey(t => t.ProjectObjectiveID)
                .ForeignKey("dbo.Administrators", t => t.CreatedByAdministratorID)
                .ForeignKey("dbo.Administrators", t => t.LastUpdatedByAdministratorID)
                .ForeignKey("dbo.MunicipalProjects", t => t.ProjectID)
                .Index(t => t.ProjectID)
                .Index(t => t.CreatedByAdministratorID)
                .Index(t => t.LastUpdatedByAdministratorID);
            
            CreateTable(
                "dbo.ProjectProgresses",
                c => new
                    {
                        ProjectProgressID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        ProgressPercentage = c.Int(nullable: false),
                        ProgressSummary = c.String(nullable: false, maxLength: 4000),
                        CurrentActivity = c.String(maxLength: 1000),
                        IssuesEncountered = c.String(maxLength: 2000),
                        NextPlannedActivity = c.String(maxLength: 2000),
                        ProgressDate = c.DateTime(nullable: false),
                        DateRecorded = c.DateTime(nullable: false),
                        RecordedByAdministratorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProjectProgressID)
                .ForeignKey("dbo.MunicipalProjects", t => t.ProjectID)
                .ForeignKey("dbo.Administrators", t => t.RecordedByAdministratorID)
                .Index(t => t.ProjectID)
                .Index(t => t.RecordedByAdministratorID);
            
            CreateTable(
                "dbo.ProjectRequests",
                c => new
                    {
                        ProjectRequestID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        RequestID = c.Int(nullable: false),
                        RelationshipType = c.String(maxLength: 100),
                        DateLinked = c.DateTime(nullable: false),
                        LinkedByAdministratorID = c.Int(nullable: false),
                        Notes = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.ProjectRequestID)
                .ForeignKey("dbo.Administrators", t => t.LinkedByAdministratorID)
                .ForeignKey("dbo.MunicipalProjects", t => t.ProjectID)
                .ForeignKey("dbo.Requests", t => t.RequestID)
                .Index(t => new { t.ProjectID, t.RequestID }, unique: true, name: "IX_ProjectRequest_Project_Request")
                .Index(t => t.LinkedByAdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProjectRequests", "RequestID", "dbo.Requests");
            DropForeignKey("dbo.ProjectRequests", "ProjectID", "dbo.MunicipalProjects");
            DropForeignKey("dbo.ProjectRequests", "LinkedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ProjectProgresses", "RecordedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ProjectProgresses", "ProjectID", "dbo.MunicipalProjects");
            DropForeignKey("dbo.ProjectObjectives", "ProjectID", "dbo.MunicipalProjects");
            DropForeignKey("dbo.ProjectObjectives", "LastUpdatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ProjectObjectives", "CreatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ProjectMilestones", "ProjectID", "dbo.MunicipalProjects");
            DropForeignKey("dbo.ProjectMilestones", "LastUpdatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ProjectMilestones", "CreatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ProjectHistories", "ProjectID", "dbo.MunicipalProjects");
            DropForeignKey("dbo.ProjectHistories", "PerformedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ProjectEvidences", "RecordedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ProjectEvidences", "ProjectID", "dbo.MunicipalProjects");
            DropIndex("dbo.ProjectRequests", new[] { "LinkedByAdministratorID" });
            DropIndex("dbo.ProjectRequests", "IX_ProjectRequest_Project_Request");
            DropIndex("dbo.ProjectProgresses", new[] { "RecordedByAdministratorID" });
            DropIndex("dbo.ProjectProgresses", new[] { "ProjectID" });
            DropIndex("dbo.ProjectObjectives", new[] { "LastUpdatedByAdministratorID" });
            DropIndex("dbo.ProjectObjectives", new[] { "CreatedByAdministratorID" });
            DropIndex("dbo.ProjectObjectives", new[] { "ProjectID" });
            DropIndex("dbo.ProjectMilestones", new[] { "LastUpdatedByAdministratorID" });
            DropIndex("dbo.ProjectMilestones", new[] { "CreatedByAdministratorID" });
            DropIndex("dbo.ProjectMilestones", new[] { "ProjectID" });
            DropIndex("dbo.ProjectHistories", new[] { "PerformedByAdministratorID" });
            DropIndex("dbo.ProjectHistories", new[] { "ProjectID" });
            DropIndex("dbo.ProjectEvidences", new[] { "RecordedByAdministratorID" });
            DropIndex("dbo.ProjectEvidences", new[] { "ProjectID" });
            DropTable("dbo.ProjectRequests");
            DropTable("dbo.ProjectProgresses");
            DropTable("dbo.ProjectObjectives");
            DropTable("dbo.ProjectMilestones");
            DropTable("dbo.ProjectHistories");
            DropTable("dbo.ProjectEvidences");
        }
    }
}
