namespace NovaSaaS.Domain.Enums
{
    public enum DocumentStatus
    {
        Draft,
        Published,
        UnderReview,
        Approved,
        Archived,
        Deleted
    }

    public enum DocumentPermissionLevel
    {
        View,
        Comment,
        Edit,
        FullControl
    }

    public enum DocumentWorkflowStatus
    {
        Pending,
        InReview,
        Approved,
        Rejected,
        Cancelled
    }

    public enum WorkflowStepAction
    {
        Approve,
        Reject,
        RequestChanges,
        Escalate
    }

    public enum CheckoutStatus
    {
        CheckedOut,
        CheckedIn
    }

    public enum ShareType
    {
        User,
        Role,
        Department,
        Public
    }

    public enum FolderType
    {
        General,
        Project,
        Department,
        Personal,
        Template,
        Archive
    }
}
