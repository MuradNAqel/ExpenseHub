
namespace Expenses.Api.Core.Models;

public class ExpenseItem
{
    public long Id { get; set; }

    public long ExpenseClaimId { get; set; }

    public ExpenseCategory Category { get; set; } = ExpenseCategory.Travel;

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateOnly ExpenseDate { get; set; }
}
