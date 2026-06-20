namespace Expenses.Api.Application.Dtos;

public sealed class ExpenseDashboardResponse
{
    public int TotalClaims { get; init; }
    public int PendingClaims { get; init; }
    public int ApprovedClaims { get; init; }
    public int RejectedClaims { get; init; }
    public decimal TotalRequestedAmount { get; init; }
    public decimal PendingAmount { get; init; }
    public decimal ApprovedAmount { get; init; }
    public decimal AverageClaimAmount { get; init; }
    public IReadOnlyList<ExpenseStatusTotalResponse> StatusTotals { get; init; } = [];
    public IReadOnlyList<ExpenseMonthlyTotalResponse> MonthlyTotals { get; init; } = [];
    public IReadOnlyList<ExpenseCategoryTotalResponse> CategoryTotals { get; init; } = [];
    public IReadOnlyList<ExpenseEmployeeTotalResponse> TopEmployees { get; init; } = [];
    public IReadOnlyList<ExpenseClaimSummaryResponse> RecentClaims { get; init; } = [];
}

public sealed class ExpenseStatusTotalResponse
{
    public ExpenseClaimStatus Status { get; init; }
    public int ClaimCount { get; init; }
    public decimal TotalAmount { get; init; }
}

public sealed class ExpenseMonthlyTotalResponse
{
    public string Month { get; init; } = string.Empty;
    public int ClaimCount { get; init; }
    public decimal TotalAmount { get; init; }
}

public sealed class ExpenseCategoryTotalResponse
{
    public ExpenseCategory Category { get; init; }
    public int ItemCount { get; init; }
    public decimal TotalAmount { get; init; }
}

public sealed class ExpenseEmployeeTotalResponse
{
    public long EmployeeId { get; init; }
    public int ClaimCount { get; init; }
    public decimal TotalAmount { get; init; }
}
