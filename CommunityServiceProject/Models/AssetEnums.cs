namespace CommunityServiceProject.Models
{
    public enum AssetCondition
    {
        Excellent,
        Good,
        Fair,
        Poor,
        Critical
    }

    public enum AssetStatus
    {
        Active,
        UnderMaintenance,
        RequiresAttention,
        Inactive,
        Retired
    }

    public enum AssetMaintenanceNeedStatus
    {
        Identified,
        Planned,
        InProgress,
        Resolved,
        Cancelled
    }

    public enum AssetMaintenancePriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}