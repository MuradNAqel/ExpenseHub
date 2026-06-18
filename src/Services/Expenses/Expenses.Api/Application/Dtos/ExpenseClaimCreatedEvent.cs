namespace Expenses.Api.Application.Dtos;

public sealed record ExpenseClaimCreatedEvent(
        long ExpenseClaimId,
        long EmployeeId,
        string EmployeeName,
        string Title,
        decimal TotalAmount,
        string Status,
        DateTime CreatedAt
);