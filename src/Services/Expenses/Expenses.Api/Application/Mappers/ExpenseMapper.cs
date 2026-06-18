using Expenses.Api.Application.Dtos;
using Expenses.Api.Core.Enums;

namespace Expenses.Api.Application.Mappers;

internal static class ExpenseMapper
{
    public static ExpenseClaimSummaryResponse MapSummary(ExpenseClaimRow row)
    {
        return new ExpenseClaimSummaryResponse
        {
            Id = row.Id,
            EmployeeId = row.EmployeeId,
            Title = row.Title,
            TotalAmount = row.TotalAmount,
            Status = ParseStatus(row.Status),
            CreatedAt = row.CreatedAt,
            ReviewedAt = row.ReviewedAt,
            RejectionReason = row.RejectionReason
        };
    }

    public static ExpenseClaimDetailsResponse MapDetails(ExpenseClaimRow row, IReadOnlyList<ExpenseItemResponse> items)
    {
        return new ExpenseClaimDetailsResponse
        {
            Id = row.Id,
            EmployeeId = row.EmployeeId,
            Title = row.Title,
            TotalAmount = row.TotalAmount,
            Status = ParseStatus(row.Status),
            CreatedAt = row.CreatedAt,
            ReviewedAt = row.ReviewedAt,
            RejectionReason = row.RejectionReason,
            Items = items
        };
    }

    public static ExpenseItemResponse MapItem(ExpenseItemRow row)
    {
        return new ExpenseItemResponse
        {
            Id = row.Id,
            ExpenseClaimId = row.ExpenseClaimId,
            Category = ParseCategory(row.Category),
            Amount = row.Amount,
            Description = row.Description,
            ExpenseDate = DateOnly.FromDateTime(row.ExpenseDate)
        };
    }

    private static ExpenseClaimStatus ParseStatus(string status)
    {
        if (Enum.TryParse<ExpenseClaimStatus>(status, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Unknown expense claim status '{status}'.");
    }

    private static ExpenseCategory ParseCategory(string category)
    {
        if (Enum.TryParse<ExpenseCategory>(category, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Unknown expense category '{category}'.");
    }
}
