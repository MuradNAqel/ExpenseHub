using Dapper;
using Expenses.Api.Application.Abstractions;
using Expenses.Api.Application.Dtos;
using Expenses.Api.Application.Mappers;
using Expenses.Api.Core.Interfaces;

namespace Expenses.Api.Application.V1.Queries;

public sealed record GetExpenseClaimByIdQuery(long Id);

public sealed class GetExpenseClaimByIdQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetExpenseClaimByIdQuery, ExpenseClaimDetailsResponse?>
{
    public async Task<ExpenseClaimDetailsResponse?> HandleAsync(GetExpenseClaimByIdQuery query)
    {
        using var connection = connectionFactory.CreateConnection();

        const string claimSql = """
            SELECT
                Id,
                EmployeeId,
                Title,
                TotalAmount,
                Status,
                CreatedAt,
                ReviewedAt,
                RejectionReason
            FROM ExpenseClaims
            WHERE Id = @Id;
            """;

        const string itemsSql = """
            SELECT
                Id,
                ExpenseClaimId,
                Category,
                Amount,
                Description,
                ExpenseDate
            FROM ExpenseItems
            WHERE ExpenseClaimId = @Id
            ORDER BY ExpenseDate DESC;
            """;

        var claim = await connection.QuerySingleOrDefaultAsync<ExpenseClaimRow>(
            claimSql,
            new { query.Id });

        if (claim is null)
            return null;

        var items = (await connection.QueryAsync<ExpenseItemRow>(
            itemsSql,
            new { query.Id })).Select(ExpenseMapper.MapItem).ToList();

        return ExpenseMapper.MapDetails(claim, items);
    }
}
