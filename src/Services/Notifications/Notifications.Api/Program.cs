using System.Text.Json.Serialization;
using EventBus.Events;
using EventBus.Interfaces;
using EventBus.RabbitMQ;
using NotificationCore.Telegram;
using Notifications.Api.Application.Abstractions;
using Notifications.Api.Application.Commands;
using Notifications.Api.Application.Dtos;
using Notifications.Api.Application.Events;
using Notifications.Api.Application.V1.Queries;
using Notifications.Api.Infrastructure.Data;
using Notifications.Api.Infrastructure.Messaging;
using Notifications.Api.Infrastructure.Services;
using Scalar.AspNetCore;
using WebApi.Errors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddGlobalExceptionHandling();
builder.Services.AddOpenApi();
builder.Services.AddRabbitMqEventBus(builder.Configuration);
builder.Services.AddTelegramNotifications(builder.Configuration);
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services
    .AddScoped<ICommandHandler<CreateNotificationCommand, NotificationMessageResponse>,
        CreateNotificationCommandHandler>();
builder.Services
    .AddScoped<IQueryHandler<GetAllNotificationMessagesQuery, IReadOnlyList<NotificationMessageResponse>>,
        GetAllNotificationMessagesQueryHandler>();
builder.Services
    .AddScoped<IQueryHandler<GetNotificationMessageByIdQuery, NotificationMessageResponse?>,
        GetNotificationMessageByIdQueryHandler>();
builder.Services
    .AddScoped<IIntegrationEventHandler<ExpenseClaimCreatedEvent>, ExpenseClaimCreatedEventHandler>();
builder.Services.AddHostedService<ExpenseClaimCreatedConsumer>();
var app = builder.Build();

await app.Services.InitializeNotificationsDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

}

//app.UseHttpsRedirection();

app.UseGlobalExceptionHandling();
app.MapControllers();

app.Run();
