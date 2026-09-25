using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class ProjectEvidence
    {
        [Key]
        public int ProjectEvidenceID { get; set; }


        [Required]
        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public virtual MunicipalProject Project { get; set; }


        [Required]
        [StringLength(200, MinimumLength = 3)]
        [Display(Name = "Evidence Title")]
        public string EvidenceTitle { get; set; }


        [StringLength(2000)]
        [Display(Name = "Description")]
        public string Description { get; set; }


        [Required]
        [StringLength(50)]
        [Display(Name = "Evidence Type")]
        public string EvidenceType { get; set; }


        [Required]
        [StringLength(500)]
        [Display(Name = "File Name")]
        public string FileName { get; set; }


        [Required]
        [StringLength(1000)]
        [Display(Name = "File Path")]
        public string FilePath { get; set; }


        [StringLength(150)]
        [Display(Name = "Content Type")]
        public string ContentType { get; set; }


        [Display(Name = "File Size")]
        public long? FileSize { get; set; }


        [Required]
        [Display(Name = "Date Recorded")]
        public DateTime DateRecorded { get; set; }


        [Required]
        [Display(Name = "Recorded By")]
        public int RecordedByAdministratorID { get; set; }

        [ForeignKey("RecordedByAdministratorID")]
        public virtual Administrator RecordedByAdministrator { get; set; }
    }
}