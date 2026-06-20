
namespace Expenses.Api.Application.Dtos;

public class CreateExpenseItemRequest
{
    public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateOnly ExpenseDate { get; set; }
}
