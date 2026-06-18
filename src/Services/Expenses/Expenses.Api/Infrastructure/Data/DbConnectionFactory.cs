using System.Data;
using Microsoft.Data.SqlClient;

namespace Expenses.Api.Infrastructure.Data;

public class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connectionString = configuration.GetConnectionString("SqlServerConnection");
        return new SqlConnection(connectionString);
    }
}