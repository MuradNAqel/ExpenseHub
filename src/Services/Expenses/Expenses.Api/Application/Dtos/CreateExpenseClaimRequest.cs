namespace Expenses.Api.Application.Dtos;

public class CreateExpenseClaimRequest
{
    public long EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<CreateExpenseItemRequest> Items { get; set; } = [];
}