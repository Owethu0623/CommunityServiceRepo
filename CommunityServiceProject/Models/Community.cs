using System.Data.Entity;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace CommunityServiceProject.Models
{
    public class Community : DbContext
    {
        public Community() : base("Community")
        {
        }

        // Existing entities
        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Ward> Wards { get; set; }

        // Technician skills
        public DbSet<Skill> Skills { get; set; }
        public DbSet<TechnicianSkill> TechnicianSkills { get; set; }
        public DbSet<RequestSkill> RequestSkills { get; set; }

        // Technician assignments
        public DbSet<TechnicianAssignment> TechnicianAssignments { get; set; }
        public DbSet<AssignmentIssue> AssignmentIssues { get; set; }
        public DbSet<ReassignmentRequest> ReassignmentRequests { get; set; }

        // Maintenance
        public DbSet<MaintenanceWork> MaintenanceWorks { get; set; }
        public DbSet<MaintenanceProgress> MaintenanceProgress { get; set; }
        public DbSet<WorkNote> WorkNotes { get; set; }
        public DbSet<MaintenanceMaterial> MaintenanceMaterials { get; set; }
        public DbSet<MaintenanceEvidence> MaintenanceEvidence { get; set; }
        public DbSet<MaintenanceCompletion> MaintenanceCompletions { get; set; }

        // Maintenance knowledge base
        public DbSet<MaintenanceKnowledgeBase> MaintenanceKnowledgeBases { get; set; }

         // Compliance
        public DbSet<ComplianceRecord> ComplianceRecords { get; set; }
        public DbSet<Violation> Violations { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<AccountRestriction> AccountRestrictions { get; set; }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // REQUEST RELATIONSHIPS
            // =========================================================

            // Request -> Citizen
            modelBuilder.Entity<Request>()
                .HasRequired(r => r.Citizen)
                .WithMany()
                .HasForeignKey(r => r.CitizenID)
                .WillCascadeOnDelete(false);

            // Request -> Category
            modelBuilder.Entity<Request>()
                .HasRequired(r => r.Category)
                .WithMany(c => c.Requests)
                .HasForeignKey(r => r.CategoryID)
                .WillCascadeOnDelete(false);

            // Request -> Ward
            modelBuilder.Entity<Request>()
                .HasRequired(r => r.Ward)
                .WithMany(w => w.Requests)
                .HasForeignKey(r => r.WardID)
                .WillCascadeOnDelete(false);

            // Request -> Administrator
            modelBuilder.Entity<Request>()
                .HasOptional(r => r.Administrator)
                .WithMany(a => a.Requests)
                .HasForeignKey(r => r.AdministratorID)
                .WillCascadeOnDelete(false);

            // Request -> current Technician
            modelBuilder.Entity<Request>()
                .HasOptional(r => r.Technician)
                .WithMany(t => t.Requests)
                .HasForeignKey(r => r.TechnicianID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // SKILL RELATIONSHIPS
            // =========================================================

            // TechnicianSkill -> Technician
            modelBuilder.Entity<TechnicianSkill>()
                .HasRequired(ts => ts.Technician)
                .WithMany(t => t.TechnicianSkills)
                .HasForeignKey(ts => ts.TechnicianID)
                .WillCascadeOnDelete(false);

            // TechnicianSkill -> Skill
            modelBuilder.Entity<TechnicianSkill>()
                .HasRequired(ts => ts.Skill)
                .WithMany(s => s.TechnicianSkills)
                .HasForeignKey(ts => ts.SkillID)
                .WillCascadeOnDelete(false);

            // RequestSkill -> Request
            modelBuilder.Entity<RequestSkill>()
                .HasRequired(rs => rs.Request)
                .WithMany(r => r.RequiredSkills)
                .HasForeignKey(rs => rs.RequestID)
                .WillCascadeOnDelete(false);

            // RequestSkill -> Skill
            modelBuilder.Entity<RequestSkill>()
                .HasRequired(rs => rs.Skill)
                .WithMany()
                .HasForeignKey(rs => rs.SkillID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // TECHNICIAN ASSIGNMENTS
            // =========================================================

            // TechnicianAssignment -> Request
            modelBuilder.Entity<TechnicianAssignment>()
                .HasRequired(a => a.Request)
                .WithMany(r => r.TechnicianAssignments)
                .HasForeignKey(a => a.RequestID)
                .WillCascadeOnDelete(false);

            // TechnicianAssignment -> Technician
            modelBuilder.Entity<TechnicianAssignment>()
                .HasRequired(a => a.Technician)
                .WithMany(t => t.TechnicianAssignments)
                .HasForeignKey(a => a.TechnicianID)
                .WillCascadeOnDelete(false);

            // TechnicianAssignment -> Administrator
            modelBuilder.Entity<TechnicianAssignment>()
                .HasRequired(a => a.Administrator)
                .WithMany(ad => ad.TechnicianAssignments)
                .HasForeignKey(a => a.AdministratorID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // ASSIGNMENT ISSUES
            // =========================================================

            // AssignmentIssue -> TechnicianAssignment
            modelBuilder.Entity<AssignmentIssue>()
                .HasRequired(i => i.Assignment)
                .WithMany(a => a.AssignmentIssues)
                .HasForeignKey(i => i.AssignmentID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // REASSIGNMENT REQUESTS
            // =========================================================

            // ReassignmentRequest -> Assignment
            modelBuilder.Entity<ReassignmentRequest>()
                .HasRequired(r => r.Assignment)
                .WithMany(a => a.ReassignmentRequests)
                .HasForeignKey(r => r.AssignmentID)
                .WillCascadeOnDelete(false);

            // ReassignmentRequest -> Technician
            modelBuilder.Entity<ReassignmentRequest>()
                .HasRequired(r => r.Technician)
                .WithMany()
                .HasForeignKey(r => r.TechnicianID)
                .WillCascadeOnDelete(false);

            // ReassignmentRequest -> reviewing Administrator
            modelBuilder.Entity<ReassignmentRequest>()
                .HasOptional(r => r.ReviewedByAdministrator)
                .WithMany(a => a.ReassignmentRequests)
                .HasForeignKey(r => r.ReviewedByAdministratorID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // MAINTENANCE
            // =========================================================

            // MaintenanceWork -> Request
            modelBuilder.Entity<MaintenanceWork>()
                .HasRequired(m => m.Request)
                .WithMany(r => r.MaintenanceWorks)
                .HasForeignKey(m => m.RequestID)
                .WillCascadeOnDelete(false);

            // MaintenanceWork -> Technician
            modelBuilder.Entity<MaintenanceWork>()
                .HasRequired(m => m.Technician)
                .WithMany()
                .HasForeignKey(m => m.TechnicianID)
                .WillCascadeOnDelete(false);

            // MaintenanceProgress -> MaintenanceWork
            modelBuilder.Entity<MaintenanceProgress>()
                .HasRequired(p => p.MaintenanceWork)
                .WithMany(m => m.ProgressRecords)
                .HasForeignKey(p => p.MaintenanceWorkID)
                .WillCascadeOnDelete(false);

            // WorkNote -> MaintenanceWork
            modelBuilder.Entity<WorkNote>()
                .HasRequired(n => n.MaintenanceWork)
                .WithMany(m => m.WorkNotes)
                .HasForeignKey(n => n.MaintenanceWorkID)
                .WillCascadeOnDelete(false);

            // MaintenanceMaterial -> MaintenanceWork
            modelBuilder.Entity<MaintenanceMaterial>()
                .HasRequired(m => m.MaintenanceWork)
                .WithMany(w => w.Materials)
                .HasForeignKey(m => m.MaintenanceWorkID)
                .WillCascadeOnDelete(false);

            // MaintenanceEvidence -> MaintenanceWork
            modelBuilder.Entity<MaintenanceEvidence>()
                .HasRequired(e => e.MaintenanceWork)
                .WithMany(m => m.Evidence)
                .HasForeignKey(e => e.MaintenanceWorkID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // MAINTENANCE COMPLETION
            // =========================================================

            // MaintenanceCompletion -> MaintenanceWork
            modelBuilder.Entity<MaintenanceCompletion>()
                .HasRequired(c => c.MaintenanceWork)
                .WithMany(m => m.Completions)
                .HasForeignKey(c => c.MaintenanceWorkID)
                .WillCascadeOnDelete(false);

            // MaintenanceCompletion -> verifying Administrator
            modelBuilder.Entity<MaintenanceCompletion>()
                .HasOptional(c => c.VerifiedByAdministrator)
                .WithMany(a => a.MaintenanceCompletions)
                .HasForeignKey(c => c.VerifiedByAdministratorID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // KNOWLEDGE BASE
            // =========================================================

            // Knowledge base -> MaintenanceCompletion
            modelBuilder.Entity<MaintenanceKnowledgeBase>()
                .HasRequired(k => k.MaintenanceCompletion)
                .WithMany(c => c.KnowledgeBaseEntries)
                .HasForeignKey(k => k.MaintenanceCompletionID)
                .WillCascadeOnDelete(false);

            // Knowledge base -> Category
            modelBuilder.Entity<MaintenanceKnowledgeBase>()
                .HasRequired(k => k.Category)
                .WithMany()
                .HasForeignKey(k => k.CategoryID)
                .WillCascadeOnDelete(false);

            // Knowledge base -> creating Technician
            modelBuilder.Entity<MaintenanceKnowledgeBase>()
                .HasRequired(k => k.CreatedByTechnician)
                .WithMany()
                .HasForeignKey(k => k.CreatedByTechnicianID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // COMPLIANCE
            // =========================================================

            
            // ComplianceRecord -> Citizen
            modelBuilder.Entity<ComplianceRecord>()
                .HasRequired(c => c.Citizen)
                .WithMany()
                .HasForeignKey(c => c.CitizenID)
                .WillCascadeOnDelete(false);

            // Violation -> ComplianceRecord
            modelBuilder.Entity<Violation>()
                .HasRequired(v => v.ComplianceRecord)
                .WithMany(c => c.Violations)
                .HasForeignKey(v => v.ComplianceID)
                .WillCascadeOnDelete(false);

            // Violation -> Request
            modelBuilder.Entity<Violation>()
                .HasOptional(v => v.Request)
                .WithMany()
                .HasForeignKey(v => v.RequestID)
                .WillCascadeOnDelete(false);

            // Violation -> Administrator
            modelBuilder.Entity<Violation>()
                .HasRequired(v => v.Administrator)
                .WithMany()
                .HasForeignKey(v => v.AdministratorID)
                .WillCascadeOnDelete(false);

            // Warning -> Violation
            modelBuilder.Entity<Warning>()
                .HasRequired(w => w.Violation)
                .WithMany(v => v.Warnings)
                .HasForeignKey(w => w.ViolationID)
                .WillCascadeOnDelete(false);

            // Warning -> Administrator
            modelBuilder.Entity<Warning>()
                .HasRequired(w => w.Administrator)
                .WithMany()
                .HasForeignKey(w => w.AdministratorID)
                .WillCascadeOnDelete(false);

            // AccountRestriction -> Citizen
            modelBuilder.Entity<AccountRestriction>()
                .HasRequired(r => r.Citizen)
                .WithMany(c => c.AccountRestrictions)
                .HasForeignKey(r => r.CitizenID)
                .WillCascadeOnDelete(false);

            // AccountRestriction -> Administrator
            modelBuilder.Entity<AccountRestriction>()
                .HasRequired(r => r.Administrator)
                .WithMany()
                .HasForeignKey(r => r.AdministratorID)
                .WillCascadeOnDelete(false);

        }
    }
}
