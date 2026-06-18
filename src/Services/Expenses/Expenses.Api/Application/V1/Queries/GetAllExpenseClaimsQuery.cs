using Dapper;
using Expenses.Api.Application.Abstractions;
using Expenses.Api.Application.Dtos;
using Expenses.Api.Application.Mappers;
using Expenses.Api.Core.Interfaces;

namespace Expenses.Api.Application.V1.Queries;

public sealed record GetAllExpenseClaimsQuery;

public sealed class GetAllExpenseClaimsQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetAllExpenseClaimsQuery, IReadOnlyList<ExpenseClaimSummaryResponse>>
{
    public async Task<IReadOnlyList<ExpenseClaimSummaryResponse>> HandleAsync(GetAllExpenseClaimsQuery query)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sqlQuery = """
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
            ORDER BY CreatedAt DESC;
            """;

        var claims = await connection.QueryAsync<ExpenseClaimRow>(sqlQuery);
        return claims.Select(ExpenseMapper.MapSummary).ToList();
    }
}
