namespace Expenses.Api.Application.Dtos;

public class ExpenseClaimDetailsResponse : ExpenseClaimSummaryResponse
{
    public IReadOnlyList<ExpenseItemResponse> Items { get; init; } = [];
}
