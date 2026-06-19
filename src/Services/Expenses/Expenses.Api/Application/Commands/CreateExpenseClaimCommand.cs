using System.ComponentModel.DataAnnotations;
using Dapper;
using EventBus.Events;
using EventBus.Interfaces;
using Expenses.Api.Application.Abstractions;
using Expenses.Api.Application.Dtos;
using Expenses.Api.Core.Enums;
using Expenses.Api.Core.Interfaces;

namespace Expenses.Api.Application.Commands;

public sealed record CreateExpenseClaimCommand(CreateExpenseClaimRequest Request);
/* As if it was Please create an expense claim.
 C# auto matically translates to -->
public sealed class CreateExpenseClaimCommand
{
        public CreateExpenseClaimRequest Request { get; init; }

        public CreateExpenseClaimCommand(CreateExpenseClaimRequest request)
        {
            Request = request;
        }
}
 */
/*
  I know how to handle
CreateExpenseClaimCommand
and
I return a long
 */
public sealed class CreateExpenseClaimCommandHandler(
    IDbConnectionFactory connectionFactory,
    IEventBus eventBus)
    : ICommandHandler<CreateExpenseClaimCommand, long>
{
    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 1000;

    public async Task<long> HandleAsync(CreateExpenseClaimCommand command)
    {
        var request = command.Request;
        Validate(request);

        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            var totalAmount = request.Items.Sum(x => x.Amount);
            var title = request.Title.Trim();

            const string claimSql = """
                INSERT INTO ExpenseClaims
                    (EmployeeId, Title, TotalAmount, Status, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES
                    (@EmployeeId, @Title, @TotalAmount, @Status, SYSUTCDATETIME());
                """;

            var claimId = await connection.ExecuteScalarAsync<long>(
                claimSql,
                new
                {
                    request.EmployeeId,
                    Title = title,
                    TotalAmount = totalAmount,
                    Status = ExpenseClaimStatus.Pending.ToString()
                },
                transaction);

            const string itemSql = """
                INSERT INTO ExpenseItems
                    (ExpenseClaimId, Category, Amount, Description, ExpenseDate)
                VALUES
                    (@ExpenseClaimId, @Category, @Amount, @Description, @ExpenseDate);
                """;

            foreach (var item in request.Items)
            {
                await connection.ExecuteAsync(
                    itemSql,
                    new
                    {
                        ExpenseClaimId = claimId,
                        Category = item.Category.ToString(),
                        item.Amount,
                        Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                        ExpenseDate = item.ExpenseDate.ToDateTime(TimeOnly.MinValue)
                    },
                    transaction);
            }

            transaction.Commit();

            await eventBus.PublishAsync(new ExpenseClaimCreatedEvent(
                claimId,
                request.EmployeeId,
                title,
                totalAmount,
                ExpenseClaimStatus.Pending.ToString(),
                DateTime.UtcNow));

            return claimId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void Validate(CreateExpenseClaimRequest request)
    {
        if (request.EmployeeId <= 0)
            throw new ValidationException("EmployeeId must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");

        var title = request.Title.Trim();

        if (title.Length > MaxTitleLength)
            throw new ValidationException($"Title cannot exceed {MaxTitleLength} characters.");

        if (request.Items.Count == 0)
            throw new ValidationException("At least one expense item is required.");

        foreach (var item in request.Items)
        {
            if (!Enum.IsDefined(item.Category))
                throw new ValidationException("Category is invalid.");

            if (item.Amount <= 0)
                throw new ValidationException("Expense item amount must be greater than zero.");

            if (item.ExpenseDate == default)
                throw new ValidationException("Expense date is required.");

            if (item.Description?.Trim().Length > MaxDescriptionLength)
                throw new ValidationException($"Description cannot exceed {MaxDescriptionLength} characters.");
        }
    }
}
