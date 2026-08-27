namespace MinimalEP.Infrastructure.Data.Core;

using System.Data;

public interface IDbConnectionFactory
{
  IDbConnection CreateConnection();
}
