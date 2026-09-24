using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalAssetIndexViewModel
    {
        public List<MunicipalAsset> Assets { get; set; }

        public string SearchTerm { get; set; }

        public string SelectedType { get; set; }

        public string SelectedWard { get; set; }

        public string SelectedCondition { get; set; }

        public string SelectedStatus { get; set; }

        public int TotalAssets { get; set; }

        public int ActiveAssets { get; set; }

        public int AttentionAssets { get; set; }

        public int UnderMaintenanceAssets { get; set; }

        public int RetiredAssets { get; set; }

        public MunicipalAssetIndexViewModel()
        {
            Assets = new List<MunicipalAsset>();
        }
    }
}
