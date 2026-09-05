using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class MaintenanceKnowledgeBase
    {
        [Key]
        public int KnowledgeBaseID { get; set; }

        // Completed maintenance record used as the knowledge source
        [Required]
        public int MaintenanceCompletionID { get; set; }

        public virtual MaintenanceCompletion MaintenanceCompletion { get; set; }

        // Category of the maintenance problem
        [Required]
        public int CategoryID { get; set; }

        public virtual Category Category { get; set; }

        // Technician who created the knowledge entry
        [Required]
        public int CreatedByTechnicianID { get; set; }

        public virtual Technician CreatedByTechnician { get; set; }

        // Knowledge base information
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string ProblemDescription { get; set; }

        [Required]
        [StringLength(2000)]
        public string RecommendedSolution { get; set; }

        [StringLength(2000)]
        public string LessonsLearned { get; set; }

        [StringLength(500)]
        public string Keywords { get; set; }

        // Date the knowledge-base entry was created
        [Required]
        public DateTime CreatedDate { get; set; }

        // Indicates whether the entry is approved
        [Required]
        public bool IsApproved { get; set; }

        // Indicates whether the entry is currently available
        [Required]
        public bool IsActive { get; set; }
    }
}