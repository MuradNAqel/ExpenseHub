namespace Expenses.Api.Application.Mappers;

internal sealed class ExpenseClaimRow
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}

internal sealed class ExpenseItemRow
{
    public long Id { get; set; }
    public long ExpenseClaimId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
}
