using System.Data;
namespace Notifications.Api.Core.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}