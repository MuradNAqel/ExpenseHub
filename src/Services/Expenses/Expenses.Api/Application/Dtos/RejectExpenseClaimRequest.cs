namespace Expenses.Api.Application.Dtos;

public class RejectExpenseClaimRequest
{
    public string Reason { get; set; } = string.Empty;
}