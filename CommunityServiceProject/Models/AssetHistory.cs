using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class AssetHistory
    {
        [Key]
        public int AssetHistoryID { get; set; }

        [Required]
        public int AssetID { get; set; }

        [Required]
        public int AdministratorID { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public DateTime ActivityDate { get; set; }

        [StringLength(2000)]
        public string PreviousValue { get; set; }

        [StringLength(2000)]
        public string NewValue { get; set; }

        [ForeignKey("AssetID")]
        public virtual MunicipalAsset Asset { get; set; }

        [ForeignKey("AdministratorID")]
        public virtual Administrator Administrator { get; set; }
    }
}