using Dapper;
using Expenses.Api.Application.Abstractions;
using Expenses.Api.Application.Dtos;
using Expenses.Api.Application.Mappers;

namespace Expenses.Api.Application.V1.Queries;

public sealed record GetExpenseDashboardQuery;

public sealed class GetExpenseDashboardQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetExpenseDashboardQuery, ExpenseDashboardResponse>
{
    public async Task<ExpenseDashboardResponse> HandleAsync(GetExpenseDashboardQuery query)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                COUNT(1) AS TotalClaims,
                SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS PendingClaims,
                SUM(CASE WHEN Status = 'Approved' THEN 1 ELSE 0 END) AS ApprovedClaims,
                SUM(CASE WHEN Status = 'Rejected' THEN 1 ELSE 0 END) AS RejectedClaims,
                COALESCE(SUM(TotalAmount), 0) AS TotalRequestedAmount,
                COALESCE(SUM(CASE WHEN Status = 'Pending' THEN TotalAmount ELSE 0 END), 0) AS PendingAmount,
                COALESCE(SUM(CASE WHEN Status = 'Approved' THEN TotalAmount ELSE 0 END), 0) AS ApprovedAmount,
                COALESCE(AVG(TotalAmount), 0) AS AverageClaimAmount
            FROM ExpenseClaims;

            SELECT
                Status,
                COUNT(1) AS ClaimCount,
                COALESCE(SUM(TotalAmount), 0) AS TotalAmount
            FROM ExpenseClaims
            GROUP BY Status
            ORDER BY Status;

            SELECT TOP 6
                FORMAT(DATEFROMPARTS(YEAR(CreatedAt), MONTH(CreatedAt), 1), 'yyyy-MM') AS Month,
                COUNT(1) AS ClaimCount,
                COALESCE(SUM(TotalAmount), 0) AS TotalAmount
            FROM ExpenseClaims
            GROUP BY YEAR(CreatedAt), MONTH(CreatedAt)
            ORDER BY YEAR(CreatedAt), MONTH(CreatedAt);

            SELECT TOP 5
                Category,
                COUNT(1) AS ItemCount,
                COALESCE(SUM(Amount), 0) AS TotalAmount
            FROM ExpenseItems
            GROUP BY Category
            ORDER BY TotalAmount DESC;

            SELECT TOP 5
                EmployeeId,
                COUNT(1) AS ClaimCount,
                COALESCE(SUM(TotalAmount), 0) AS TotalAmount
            FROM ExpenseClaims
            GROUP BY EmployeeId
            ORDER BY TotalAmount DESC;

            SELECT TOP 8
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

        using var result = await connection.QueryMultipleAsync(sql);
        var totals = await result.ReadSingleAsync<ExpenseDashboardTotalsRow>();
        var statusTotals = (await result.ReadAsync<ExpenseStatusTotalRow>()).Select(MapStatusTotal).ToList();
        var monthlyTotals = (await result.ReadAsync<ExpenseMonthlyTotalResponse>()).ToList();
        var categoryTotals = (await result.ReadAsync<ExpenseCategoryTotalRow>()).Select(MapCategoryTotal).ToList();
        var topEmployees = (await result.ReadAsync<ExpenseEmployeeTotalResponse>()).ToList();
        var recentClaims = (await result.ReadAsync<ExpenseClaimRow>()).Select(ExpenseMapper.MapSummary).ToList();

        return new ExpenseDashboardResponse
        {
            TotalClaims = totals.TotalClaims,
            PendingClaims = totals.PendingClaims,
            ApprovedClaims = totals.ApprovedClaims,
            RejectedClaims = totals.RejectedClaims,
            TotalRequestedAmount = totals.TotalRequestedAmount,
            PendingAmount = totals.PendingAmount,
            ApprovedAmount = totals.ApprovedAmount,
            AverageClaimAmount = totals.AverageClaimAmount,
            StatusTotals = statusTotals,
            MonthlyTotals = monthlyTotals,
            CategoryTotals = categoryTotals,
            TopEmployees = topEmployees,
            RecentClaims = recentClaims
        };
    }

    private static ExpenseStatusTotalResponse MapStatusTotal(ExpenseStatusTotalRow row)
    {
        return new ExpenseStatusTotalResponse
        {
            Status = Enum.Parse<ExpenseClaimStatus>(row.Status, ignoreCase: true),
            ClaimCount = row.ClaimCount,
            TotalAmount = row.TotalAmount
        };
    }

    private static ExpenseCategoryTotalResponse MapCategoryTotal(ExpenseCategoryTotalRow row)
    {
        return new ExpenseCategoryTotalResponse
        {
            Category = Enum.Parse<ExpenseCategory>(row.Category, ignoreCase: true),
            ItemCount = row.ItemCount,
            TotalAmount = row.TotalAmount
        };
    }
}

internal sealed class ExpenseDashboardTotalsRow
{
    public int TotalClaims { get; set; }
    public int PendingClaims { get; set; }
    public int ApprovedClaims { get; set; }
    public int RejectedClaims { get; set; }
    public decimal TotalRequestedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal AverageClaimAmount { get; set; }
}

internal sealed class ExpenseStatusTotalRow
{
    public string Status { get; set; } = string.Empty;
    public int ClaimCount { get; set; }
    public decimal TotalAmount { get; set; }
}

internal sealed class ExpenseCategoryTotalRow
{
    public string Category { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
}
