using System.Data;
using Microsoft.Data.SqlClient;

namespace Notifications.Api.Infrastructure.Data
{
    public class DbConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
    {
        public IDbConnection CreateConnection()
        {
            var connectionString = configuration.GetConnectionString("SqlServerConnection");
            return new SqlConnection(connectionString);
        }
    }
}