using System.ComponentModel.DataAnnotations;
using Dapper;
using Expenses.Api.Application.Abstractions;
using Expenses.Api.Application.Dtos;
using Expenses.Api.Core.Enums;
using Expenses.Api.Core.Interfaces;

namespace Expenses.Api.Application.Commands;

public sealed record RejectExpenseClaimCommand(long Id, RejectExpenseClaimRequest Request);

public sealed class RejectExpenseClaimCommandHandler(IDbConnectionFactory connectionFactory)
    : ICommandHandler<RejectExpenseClaimCommand, bool>
{
    private const int MaxRejectionReasonLength = 1000;

    public async Task<bool> HandleAsync(RejectExpenseClaimCommand command)
    {
        Validate(command.Request);

        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            UPDATE ExpenseClaims
            SET Status = @RejectedStatus,
                ReviewedAt = SYSUTCDATETIME(),
                RejectionReason = @Reason
            WHERE Id = @Id
              AND Status = @PendingStatus;
            """;

        var affectedRows = await connection.ExecuteAsync(
            sql,
            new
            {
                command.Id,
                Reason = command.Request.Reason.Trim(),
                RejectedStatus = ExpenseClaimStatus.Rejected.ToString(),
                PendingStatus = ExpenseClaimStatus.Pending.ToString()
            });

        return affectedRows > 0;
    }

    private static void Validate(RejectExpenseClaimRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ValidationException("Rejection reason is required.");

        if (request.Reason.Trim().Length > MaxRejectionReasonLength)
            throw new ValidationException($"Rejection reason cannot exceed {MaxRejectionReasonLength} characters.");
    }
}
