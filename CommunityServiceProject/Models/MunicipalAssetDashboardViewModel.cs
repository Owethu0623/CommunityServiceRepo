using System;
using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalAssetDashboardViewModel
    {
        // ============================================================
        // SUMMARY COUNTS
        // ============================================================

        public int TotalAssets { get; set; }

        public int ActiveAssets { get; set; }

        public int RequiresAttention { get; set; }

        public int UnderMaintenance { get; set; }

        public int InactiveAssets { get; set; }

        public int RetiredAssets { get; set; }

        // ============================================================
        // CONDITION COUNTS
        // ============================================================

        public int ExcellentAssets { get; set; }

        public int GoodAssets { get; set; }

        public int FairAssets { get; set; }

        public int PoorAssets { get; set; }

        public int CriticalAssets { get; set; }

        // ============================================================
        // ATTENTION REQUIRED
        // ============================================================

        public List<MunicipalAsset> AttentionAssets { get; set; }

        // ============================================================
        // RECENT ASSETS
        // ============================================================

        public List<MunicipalAsset> RecentAssets { get; set; }

        // ============================================================
        // DASHBOARD DATE
        // ============================================================

        public DateTime DashboardDate { get; set; }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MunicipalAssetDashboardViewModel()
        {
            AttentionAssets =
                new List<MunicipalAsset>();

            RecentAssets =
                new List<MunicipalAsset>();

            DashboardDate =
                DateTime.Now;
        }
    }
}