using Expenses.Api.Application.Dtos;

namespace Expenses.Api.Core.Interfaces;

public interface IExpenseService 
{
    Task<IReadOnlyList<ExpenseClaimSummaryResponse>> GetAllAsync();
    Task<ExpenseClaimDetailsResponse?> GetByIdAsync(long id);
    Task<long> CreateAsync(CreateExpenseClaimRequest request);
    Task<bool> ApproveAsync(long id);
    Task<bool> RejectAsync(long id, RejectExpenseClaimRequest request);
}
