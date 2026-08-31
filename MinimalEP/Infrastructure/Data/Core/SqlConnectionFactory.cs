namespace MinimalEP.Infrastructure.Data.Core;

using System.Data;

using Microsoft.Data.SqlClient;

using MinimalEP.Infrastructure.Data;

public class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
  public IDbConnection CreateConnection()
  {
    return new SqlConnection(configuration.GetConnectionString(DatabaseOptions.ConnectionStringName));
  }
}
