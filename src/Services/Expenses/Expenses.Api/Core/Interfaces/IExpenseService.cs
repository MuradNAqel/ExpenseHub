using Expenses.Api.Application.Dtos;
using Expenses.Api.Application.V1.Queries;

namespace Expenses.Api.Core.Interfaces;

public interface IExpenseService
{
    Task<PagedResponse<ExpenseClaimSummaryResponse>> GetAllAsync(GetAllExpenseClaimsQuery query);
    Task<ExpenseClaimDetailsResponse?> GetByIdAsync(long id);
    Task<long> CreateAsync(CreateExpenseClaimRequest request);
    Task<bool> ApproveAsync(long id);
    Task<bool> RejectAsync(long id, RejectExpenseClaimRequest request);
}
