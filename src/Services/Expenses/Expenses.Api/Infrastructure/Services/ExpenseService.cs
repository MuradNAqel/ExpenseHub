using Expenses.Api.Application.Abstractions;
using Expenses.Api.Application.Commands;
using Expenses.Api.Application.Dtos;
using Expenses.Api.Application.V1.Queries;
using Expenses.Api.Core.Interfaces;

namespace Expenses.Api.Infrastructure.Services;

public class ExpenseService(
    IQueryHandler<GetAllExpenseClaimsQuery, IReadOnlyList<ExpenseClaimSummaryResponse>> getAllExpenseClaimsHandler,
    IQueryHandler<GetExpenseClaimByIdQuery, ExpenseClaimDetailsResponse?> getExpenseClaimByIdHandler,
    ICommandHandler<CreateExpenseClaimCommand, long> createExpenseClaimHandler,
    ICommandHandler<ApproveExpenseClaimCommand, bool> approveExpenseClaimHandler,
    ICommandHandler<RejectExpenseClaimCommand, bool> rejectExpenseClaimHandler) : IExpenseService
{
    public async Task<IReadOnlyList<ExpenseClaimSummaryResponse>> GetAllAsync()
    {
        return await getAllExpenseClaimsHandler.HandleAsync(new GetAllExpenseClaimsQuery());
    }

    public async Task<ExpenseClaimDetailsResponse?> GetByIdAsync(long id)
    {
        return await getExpenseClaimByIdHandler.HandleAsync(new GetExpenseClaimByIdQuery(id));
    }

    public async Task<long> CreateAsync(CreateExpenseClaimRequest request)
    {
        return await createExpenseClaimHandler.HandleAsync(new CreateExpenseClaimCommand(request));
    }

    public async Task<bool> ApproveAsync(long id)
    {
        return await approveExpenseClaimHandler.HandleAsync(new ApproveExpenseClaimCommand(id));
    }

    public async Task<bool> RejectAsync(long id, RejectExpenseClaimRequest request)
    {
        return await rejectExpenseClaimHandler.HandleAsync(new RejectExpenseClaimCommand(id, request));
    }
}
