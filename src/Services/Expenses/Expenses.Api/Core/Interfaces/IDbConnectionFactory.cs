using System.Data;

namespace Expenses.Api.Core.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}