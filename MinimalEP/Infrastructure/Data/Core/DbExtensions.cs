namespace MinimalEP.Infrastructure.Data.Core;

using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Core;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;
using MinimalEP.Infrastructure.Data.Interceptors;

public static class DbExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddApplicationData(IConfiguration configuration)
    {
      _ = configuration.GetConnectionString(DatabaseOptions.ConnectionStringName)
        ?? throw new InvalidOperationException($"ConnectionStrings:{DatabaseOptions.ConnectionStringName} is required.");

      services.AddOptions<DatabaseOptions>()
        .Bind(configuration.GetRequiredSection(DatabaseOptions.SectionName))
        .ValidateOnStart();

      // 1. Register user context for HTTP requests
      services.AddHttpContextAccessor();
      services.AddScoped<IUserContext, HttpUserContext>();
      services.AddScoped<ICustomerRepository, CustomerRepository>();
      services.AddScoped<IEmployeeRepository, EmployeeRepository>();
      services.AddScoped<IWorkloadRepository, WorkloadRepository>();
      services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

      // Dapper connection factory — Singleton because it only holds a connection string
      services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

      // 2. Register the interceptor as Singleton (IHttpContextAccessor is also Singleton
      //    and reads per-request user data from HttpContext)
      services.AddSingleton<AuditAndSoftDeleteInterceptor>();

      // 3. Register DbContext with pooling and interceptors
      services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
      {
        var connectionString = configuration.GetConnectionString(DatabaseOptions.ConnectionStringName);

        // Resolve Singleton interceptor — safe to resolve from the root provider
        var auditInterceptor = sp.GetRequiredService<AuditAndSoftDeleteInterceptor>();
        options.AddInterceptors(auditInterceptor);

        options.UseSqlServer(connectionString);
      });

      return services;
    }
  }
}
