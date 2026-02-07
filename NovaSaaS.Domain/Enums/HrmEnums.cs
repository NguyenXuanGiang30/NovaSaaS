namespace NovaSaaS.Domain.Enums
{
    public enum EmploymentStatus
    {
        Active,
        OnProbation,
        OnLeave,
        Suspended,
        Terminated,
        Retired,
        Resigned
    }

    public enum ContractType
    {
        Permanent,
        FixedTerm,
        PartTime,
        Intern,
        Freelance,
        Seasonal
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public enum MaritalStatus
    {
        Single,
        Married,
        Divorced,
        Widowed
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        EarlyLeave,
        HalfDay,
        OnLeave,
        Holiday,
        WorkFromHome
    }

    public enum LeaveRequestStatus
    {
        Pending,
        ApprovedByManager,
        Approved,
        Rejected,
        Cancelled
    }

    public enum PayrollStatus
    {
        Draft,
        Processing,
        Calculated,
        Approved,
        Paid,
        Cancelled
    }

    public enum RecruitmentStatus
    {
        Open,
        InProgress,
        OnHold,
        Closed,
        Cancelled
    }

    public enum CandidateStatus
    {
        New,
        Screening,
        Interview,
        Testing,
        Offer,
        Hired,
        Rejected,
        Withdrawn
    }

    public enum TrainingStatus
    {
        Planned,
        Enrolling,
        InProgress,
        Completed,
        Cancelled
    }

    public enum ReviewStatus
    {
        Draft,
        SelfAssessment,
        ManagerReview,
        Submitted,
        Finalized
    }

    public enum ReviewPeriodType
    {
        Monthly,
        Quarterly,
        SemiAnnual,
        Annual
    }

    public enum BonusType
    {
        Performance,
        Holiday,
        ProjectCompletion,
        Referral,
        Seniority,
        Other
    }

    public enum DeductionType
    {
        Tax,
        SocialInsurance,
        HealthInsurance,
        UnemploymentInsurance,
        Loan,
        Penalty,
        Other
    }
}
