namespace MinimalEP.Infrastructure.Data.Core;

using System.Data;

using Microsoft.Data.SqlClient;

public class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
  public IDbConnection CreateConnection()
  {
    return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
  }
}
