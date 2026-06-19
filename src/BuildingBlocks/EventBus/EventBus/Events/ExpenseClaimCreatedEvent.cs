namespace EventBus.Events;

public sealed record ExpenseClaimCreatedEvent(
    long ExpenseClaimId,
    long EmployeeId,
    string Title,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt);
