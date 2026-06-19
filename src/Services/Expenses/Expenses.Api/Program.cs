using System.Text.Json.Serialization;
using EventBus.RabbitMQ;
using Expenses.Api.Application.Abstractions;
using Expenses.Api.Application.Commands;
using Expenses.Api.Application.Dtos;
using Expenses.Api.Application.V1.Queries;
using Expenses.Api.Core.Interfaces;
using Expenses.Api.Infrastructure.Data;
using Expenses.Api.Infrastructure.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddRabbitMqEventBus(builder.Configuration);
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IQueryHandler<GetAllExpenseClaimsQuery, IReadOnlyList<ExpenseClaimSummaryResponse>>, GetAllExpenseClaimsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetExpenseClaimByIdQuery, ExpenseClaimDetailsResponse?>, GetExpenseClaimByIdQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateExpenseClaimCommand, long>, CreateExpenseClaimCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ApproveExpenseClaimCommand, bool>, ApproveExpenseClaimCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RejectExpenseClaimCommand, bool>, RejectExpenseClaimCommandHandler>();
var app = builder.Build();

await app.Services.InitializeExpenseDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();
