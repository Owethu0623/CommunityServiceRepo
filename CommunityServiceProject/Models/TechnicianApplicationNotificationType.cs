
namespace CommunityServiceProject.Models
{
    public enum TechnicianApplicationNotificationType
    {
        ApplicationSubmitted,

        DocumentReplacementRequired,
        DocumentsVerified,

        ApplicationShortlisted,

        AssessmentScheduled,
        AssessmentCompleted,

        InterviewScheduled,
        InterviewCompleted,

        ApplicationSelected,

        FinalVerificationCompleted,
        ApprovedForOnboarding,

        ApplicationNotSelected,

        OnboardingCompleted
    }
}

