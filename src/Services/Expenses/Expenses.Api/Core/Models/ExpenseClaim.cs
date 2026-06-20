
namespace Expenses.Api.Core.Models;

public class ExpenseClaim
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public ExpenseClaimStatus Status { get; set; } = ExpenseClaimStatus.Pending;

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }
}
