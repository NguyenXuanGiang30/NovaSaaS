namespace NovaSaaS.Domain.Enums
{
    public enum AccountCategory
    {
        Asset,
        Liability,
        Equity,
        Revenue,
        Expense
    }

    public enum JournalEntryStatus
    {
        Draft,
        Pending,
        Approved,
        Posted,
        Reversed
    }

    public enum JournalEntryType
    {
        Manual,
        SalesInvoice,
        PurchaseInvoice,
        Payment,
        Receipt,
        Payroll,
        StockMovement,
        Adjustment,
        Opening,
        Closing
    }

    public enum FiscalPeriodStatus
    {
        Open,
        Closed,
        Locked
    }

    public enum ReceivableStatus
    {
        Open,
        PartiallyPaid,
        Paid,
        Overdue,
        WrittenOff
    }

    public enum PayableStatus
    {
        Open,
        PartiallyPaid,
        Paid,
        Overdue,
        Disputed
    }

    public enum BankTransactionType
    {
        Deposit,
        Withdrawal,
        Transfer,
        Fee,
        Interest,
        Refund,
        Other
    }

    public enum ReconciliationStatus
    {
        Pending,
        InProgress,
        Matched,
        Unmatched,
        Reconciled
    }

    public enum BudgetStatus
    {
        Draft,
        Active,
        Frozen,
        Closed
    }

    public enum TaxType
    {
        VAT,
        IncomeTax,
        CorporateTax,
        WithholdingTax,
        ImportTax,
        Other
    }
}
