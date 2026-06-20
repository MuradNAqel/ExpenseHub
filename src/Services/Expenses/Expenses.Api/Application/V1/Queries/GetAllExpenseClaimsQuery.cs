using Dapper;
using Expenses.Api.Application.Abstractions;
using Expenses.Api.Application.Dtos;
using Expenses.Api.Application.Mappers;

namespace Expenses.Api.Application.V1.Queries;

public sealed class GetAllExpenseClaimsQuery
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public long? EmployeeId { get; init; }
    public ExpenseClaimStatus? Status { get; init; }
    public string? Search { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }

    public int GetPage() => Page.GetValueOrDefault(DefaultPage) < 1
        ? DefaultPage
        : Page.GetValueOrDefault(DefaultPage);

    public int GetPageSize() => PageSize.GetValueOrDefault(DefaultPageSize) switch
    {
        < 1 => DefaultPageSize,
        > MaxPageSize => MaxPageSize,
        var pageSize => pageSize
    };

    public int GetOffset() => (GetPage() - 1) * GetPageSize();
}

public sealed class GetAllExpenseClaimsQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetAllExpenseClaimsQuery, PagedResponse<ExpenseClaimSummaryResponse>>
{
    public async Task<PagedResponse<ExpenseClaimSummaryResponse>> HandleAsync(GetAllExpenseClaimsQuery query)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        var whereClauses = new List<string>();

        if (query.EmployeeId is not null)
        {
            whereClauses.Add("EmployeeId = @EmployeeId");
            parameters.Add("EmployeeId", query.EmployeeId.Value);
        }

        if (query.Status is not null)
        {
            whereClauses.Add("Status = @Status");
            parameters.Add("Status", query.Status.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            whereClauses.Add("(Title LIKE @Search OR CONVERT(NVARCHAR(20), Id) LIKE @Search)");
            parameters.Add("Search", $"%{query.Search.Trim()}%");
        }

        if (query.CreatedFrom is not null)
        {
            whereClauses.Add("CreatedAt >= @CreatedFrom");
            parameters.Add("CreatedFrom", query.CreatedFrom.Value);
        }

        if (query.CreatedTo is not null)
        {
            whereClauses.Add("CreatedAt <= @CreatedTo");
            parameters.Add("CreatedTo", query.CreatedTo.Value);
        }

        parameters.Add("Offset", query.GetOffset());
        parameters.Add("PageSize", query.GetPageSize());

        var whereSql = whereClauses.Count == 0
            ? string.Empty
            : $"WHERE {string.Join(" AND ", whereClauses)}";

        var sqlQuery = $"""
            SELECT COUNT(1)
            FROM ExpenseClaims
            {whereSql};

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
            {whereSql}
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var result = await connection.QueryMultipleAsync(sqlQuery, parameters);
        var totalCount = await result.ReadSingleAsync<int>();
        var claims = await result.ReadAsync<ExpenseClaimRow>();

        return new PagedResponse<ExpenseClaimSummaryResponse>
        {
            Items = claims.Select(ExpenseMapper.MapSummary).ToList(),
            Page = query.GetPage(),
            PageSize = query.GetPageSize(),
            TotalCount = totalCount
        };
    }
}
