using Microsoft.Data.SqlClient;

namespace Expenses.Api.Infrastructure.Data;

public static class ExpenseDatabaseInitializer
{
    public static async Task InitializeExpenseDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(ExpenseDatabaseInitializer));

        var connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new InvalidOperationException("Connection string 'SqlServerConnection' is not configured.");

        await EnsureDatabaseAsync(connectionString);
        await EnsureSchemaAsync(connectionString);
        logger.LogInformation("Expense database schema is ready.");
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
            IF OBJECT_ID(N'dbo.ExpenseClaims', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ExpenseClaims
                (
                    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExpenseClaims PRIMARY KEY,
                    EmployeeId BIGINT NOT NULL,
                    Title NVARCHAR(200) NOT NULL,
                    TotalAmount DECIMAL(18, 2) NOT NULL,
                    Status NVARCHAR(50) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL,
                    ReviewedAt DATETIME2 NULL,
                    RejectionReason NVARCHAR(1000) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.ExpenseItems', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ExpenseItems
                (
                    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExpenseItems PRIMARY KEY,
                    ExpenseClaimId BIGINT NOT NULL,
                    Category NVARCHAR(100) NOT NULL,
                    Amount DECIMAL(18, 2) NOT NULL,
                    Description NVARCHAR(1000) NULL,
                    ExpenseDate DATE NOT NULL,
                    CONSTRAINT FK_ExpenseItems_ExpenseClaims
                        FOREIGN KEY (ExpenseClaimId)
                        REFERENCES dbo.ExpenseClaims(Id)
                        ON DELETE CASCADE
                );
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_ExpenseItems_ExpenseClaimId'
                  AND object_id = OBJECT_ID(N'dbo.ExpenseItems')
            )
            BEGIN
                CREATE INDEX IX_ExpenseItems_ExpenseClaimId
                    ON dbo.ExpenseItems(ExpenseClaimId);
            END;
            """;

        await command.ExecuteNonQueryAsync();
    }
}
