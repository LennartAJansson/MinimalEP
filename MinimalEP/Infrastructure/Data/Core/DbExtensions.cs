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
      // 1. Registrera användarkontexten för HTTP-anrop
      services.AddHttpContextAccessor();
      services.AddScoped<IUserContext, HttpUserContext>();
      services.AddScoped<ICustomerRepository, CustomerRepository>();
      services.AddScoped<IEmployeeRepository, EmployeeRepository>();
      services.AddScoped<IWorkloadRepository, WorkloadRepository>();

      // Dapper connection factory – Singleton eftersom den bara håller connection string
      services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

      // 2. Registrera vår interceptor som Singleton (IHttpContextAccessor är singleton
      //    och läser användardata per anrop via HttpContext)
      services.AddSingleton<AuditAndSoftDeleteInterceptor>();

      // 3. Registrera vår DbContext med pooling och interceptors
      services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
      {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Hämta vår Singleton interceptor – säkert att lösa från root provider
        var auditInterceptor = sp.GetRequiredService<AuditAndSoftDeleteInterceptor>();
        options.AddInterceptors(auditInterceptor);

        options.UseSqlServer(connectionString);

#if DEBUG
        // Bra för utveckling, men stängs av automatiskt i produktion för prestanda
        options.EnableSensitiveDataLogging();
#endif
      });

      return services;
    }
  }
}
