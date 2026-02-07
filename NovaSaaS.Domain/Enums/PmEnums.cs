namespace NovaSaaS.Domain.Enums
{
    public enum ProjectStatus
    {
        Planning,
        Active,
        OnHold,
        Completed,
        Cancelled,
        Archived
    }

    public enum ProjectPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum ProjectTaskStatus
    {
        Backlog,
        Todo,
        InProgress,
        Review,
        Testing,
        Done,
        Cancelled
    }

    public enum ProjectTaskPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public enum MilestoneStatus
    {
        Pending,
        InProgress,
        Completed,
        Overdue,
        Cancelled
    }

    public enum ProjectRole
    {
        Manager,
        Lead,
        Member,
        Observer,
        Stakeholder
    }

    public enum TimesheetStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected
    }

    public enum ProjectExpenseStatus
    {
        Pending,
        Approved,
        Rejected,
        Reimbursed
    }

    public enum ProjectBillingType
    {
        FixedPrice,
        TimeAndMaterial,
        NonBillable
    }
}
