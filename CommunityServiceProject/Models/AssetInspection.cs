using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class AssetInspection
    {
        [Key]
        public int InspectionID { get; set; }

        [Required]
        public int AssetID { get; set; }

        [Required]
        public int AdministratorID { get; set; }

        [Required]
        public DateTime InspectionDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Condition { get; set; }

        [Required]
        [StringLength(2000)]
        public string Findings { get; set; }

        [StringLength(2000)]
        public string InspectorNotes { get; set; }

        [StringLength(100)]
        public string RecommendedAction { get; set; }

        public DateTime? NextInspectionDate { get; set; }

        [ForeignKey("AssetID")]
        public virtual MunicipalAsset Asset { get; set; }

        [ForeignKey("AdministratorID")]
        public virtual Administrator Administrator { get; set; }
    }
}