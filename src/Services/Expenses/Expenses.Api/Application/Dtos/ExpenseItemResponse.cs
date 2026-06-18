using Expenses.Api.Core.Enums;

namespace Expenses.Api.Application.Dtos;

public class ExpenseItemResponse
{
    public long Id { get; init; }
    public long ExpenseClaimId { get; init; }
    public ExpenseCategory Category { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public DateOnly ExpenseDate { get; init; }
}
