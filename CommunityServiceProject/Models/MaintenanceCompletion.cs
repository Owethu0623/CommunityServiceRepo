using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class MaintenanceCompletion
    {
        [Key]
        public int MaintenanceCompletionID { get; set; }

        // Maintenance work relationship
        [Required]
        public int MaintenanceWorkID { get; set; }

        public virtual MaintenanceWork MaintenanceWork { get; set; }

        // Completion information
        [Required]
        [StringLength(2000)]
        public string MaintenanceSummary { get; set; }

        [Required]
        [StringLength(2000)]
        public string ResolutionAction { get; set; }

        // Date and time completion was submitted
        [Required]
        public DateTime SubmittedDate { get; set; }

        // Administrator verification
        [Required]
        public CompletionVerificationStatus VerificationStatus { get; set; }

        public int? VerifiedByAdministratorID { get; set; }

        public virtual Administrator VerifiedByAdministrator { get; set; }

        public DateTime? VerifiedDate { get; set; }

        [StringLength(1000)]
        public string AdministratorComments { get; set; }
        public virtual ICollection<MaintenanceKnowledgeBase> KnowledgeBaseEntries { get; set; }

        public MaintenanceCompletion()
        {
            KnowledgeBaseEntries = new List<MaintenanceKnowledgeBase>();
        }
    }
}