using Expenses.Api.Application.Dtos;
using Expenses.Api.Application.V1.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Expenses.Api.Application.V1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExpensesController(IExpenseService expenseService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ExpenseClaimSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ExpenseClaimSummaryResponse>>> GetAllAsync(
        [FromQuery] GetAllExpenseClaimsQuery query)
    {
        var expenses = await expenseService.GetAllAsync(query);
        return Ok(expenses);
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ExpenseDashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpenseDashboardResponse>> GetDashboardAsync()
    {
        var dashboard = await expenseService.GetDashboardAsync();
        return Ok(dashboard);
    }

    [HttpGet("{id:long}", Name = "GetById")]
    [ProducesResponseType(typeof(ExpenseClaimDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseClaimDetailsResponse>> GetByIdAsync(long id)
    {
        var expense = await expenseService.GetByIdAsync(id);

        if (expense is null)
            return NotFound();

        return Ok(expense);
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAsync(CreateExpenseClaimRequest request)
    {
        var id = await expenseService.CreateAsync(request);
        return CreatedAtAction("GetById", new { id }, new { id });
    }

    [HttpPost("{id:long}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ApproveAsync(long id)
    {
        var approved = await expenseService.ApproveAsync(id);

        if (!approved)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id:long}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RejectAsync(long id, RejectExpenseClaimRequest request)
    {
        var rejected = await expenseService.RejectAsync(id, request);

        if (!rejected)
            return NotFound();

        return NoContent();
    }
}
