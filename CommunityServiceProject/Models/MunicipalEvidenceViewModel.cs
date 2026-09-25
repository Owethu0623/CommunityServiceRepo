using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    // ============================================================
    // US100 - PROJECT EVIDENCE PAGE
    // ============================================================

    public class MunicipalProjectEvidenceViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectType { get; set; }

        public string ProjectLocation { get; set; }

        public string WardName { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public ProjectEvidenceEntryViewModel Entry { get; set; }

        public List<ProjectEvidenceItemViewModel> EvidenceRecords { get; set; }

        public MunicipalProjectEvidenceViewModel()
        {
            Entry = new ProjectEvidenceEntryViewModel();

            EvidenceRecords =
                new List<ProjectEvidenceItemViewModel>();
        }
    }


    // ============================================================
    // US100 - NEW EVIDENCE ENTRY
    // ============================================================

    public class ProjectEvidenceEntryViewModel
    {
        public int ProjectID { get; set; }

        [Required(
            ErrorMessage =
                "Please enter an evidence title."
        )]
        [StringLength(
            200,
            MinimumLength = 3,
            ErrorMessage =
                "Evidence title must be between 3 and 200 characters."
        )]
        [Display(Name = "Evidence Title")]
        public string EvidenceTitle { get; set; }


        [StringLength(
            2000,
            ErrorMessage =
                "Description cannot exceed 2000 characters."
        )]
        [Display(Name = "Description")]
        public string Description { get; set; }


        [Required(
            ErrorMessage =
                "Please select an evidence type."
        )]
        [Display(Name = "Evidence Type")]
        public string EvidenceType { get; set; }
    }


    // ============================================================
    // US100 - EXISTING EVIDENCE RECORD
    // ============================================================

    public class ProjectEvidenceItemViewModel
    {
        public int ProjectEvidenceID { get; set; }

        public string EvidenceTitle { get; set; }

        public string Description { get; set; }

        public string EvidenceType { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string ContentType { get; set; }

        public long? FileSize { get; set; }

        public DateTime DateRecorded { get; set; }

        public string RecordedByAdministratorName { get; set; }
    }
}