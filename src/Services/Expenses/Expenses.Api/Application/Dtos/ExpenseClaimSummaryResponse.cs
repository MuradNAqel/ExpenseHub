
namespace Expenses.Api.Application.Dtos;

public class ExpenseClaimSummaryResponse
{
    public long Id { get; init; }
    public long EmployeeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public ExpenseClaimStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? RejectionReason { get; init; }
}
