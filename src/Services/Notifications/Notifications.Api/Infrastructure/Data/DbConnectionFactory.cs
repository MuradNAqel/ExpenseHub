using System.Data;
using Microsoft.Data.SqlClient;
using Notifications.Api.Core.Interfaces;

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