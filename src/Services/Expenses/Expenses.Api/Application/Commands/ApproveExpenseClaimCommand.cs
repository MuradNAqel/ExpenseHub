using Dapper;
using Expenses.Api.Application.Abstractions;
using Expenses.Api.Core.Enums;
using Expenses.Api.Core.Interfaces;

namespace Expenses.Api.Application.Commands;

public sealed record ApproveExpenseClaimCommand(long Id);

public sealed class ApproveExpenseClaimCommandHandler(IDbConnectionFactory connectionFactory)
    : ICommandHandler<ApproveExpenseClaimCommand, bool>
{
    public async Task<bool> HandleAsync(ApproveExpenseClaimCommand command)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            UPDATE ExpenseClaims
            SET Status = @ApprovedStatus,
                ReviewedAt = SYSUTCDATETIME(),
                RejectionReason = NULL
            WHERE Id = @Id
              AND Status = @PendingStatus;
            """;

        var affectedRows = await connection.ExecuteAsync(
            sql,
            new
            {
                command.Id,
                ApprovedStatus = ExpenseClaimStatus.Approved.ToString(),
                PendingStatus = ExpenseClaimStatus.Pending.ToString()
            });

        return affectedRows > 0;
    }
}
