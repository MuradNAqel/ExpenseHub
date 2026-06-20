using Microsoft.Data.SqlClient;

namespace Notifications.Api.Infrastructure.Data;

public static class NotificationsDatabaseInitializer
{
    public static async Task InitializeNotificationsDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(NotificationsDatabaseInitializer));

        var connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new InvalidOperationException("Connection string 'SqlServerConnection' is not configured.");

        await EnsureDatabaseAsync(connectionString);
        await EnsureSchemaAsync(connectionString);
        logger.LogInformation("Notifications database schema is ready.");
    }

    private static async Task EnsureDatabaseAsync(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("SqlServerConnection must include a database name.");

        builder.InitialCatalog = "master";

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        var escapedDatabaseName = databaseName.Replace("]", "]]");
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF DB_ID(N'{databaseName.Replace("'", "''")}') IS NULL
            BEGIN
                CREATE DATABASE [{escapedDatabaseName}];
            END
            """;

        await command.ExecuteNonQueryAsync();
    }

    private static async Task EnsureSchemaAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF OBJECT_ID(N'dbo.NotificationMessages', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.NotificationMessages
                (
                    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationMessages PRIMARY KEY,
                    Type NVARCHAR(200) NULL,
                    MessageChannel NVARCHAR(100) NULL,
                    Recipient NVARCHAR(500) NULL,
                    Subject NVARCHAR(500) NULL,
                    Body NVARCHAR(MAX) NULL,
                    Status NVARCHAR(100) NOT NULL,
                    ErrorMessage NVARCHAR(1000) NULL,
                    CreatedAt DATETIME2 NOT NULL,
                    SentAt DATETIME2 NULL
                );
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_NotificationMessages_CreatedAt'
                  AND object_id = OBJECT_ID(N'dbo.NotificationMessages')
            )
            BEGIN
                CREATE INDEX IX_NotificationMessages_CreatedAt
                    ON dbo.NotificationMessages(CreatedAt DESC);
            END;
            """;

        await command.ExecuteNonQueryAsync();
    }
}
