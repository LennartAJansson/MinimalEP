namespace MinimalEP.Infrastructure.Cors;

public static class CorsExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddApplicationCors(IConfiguration configuration)
    {
      var options = configuration
        .GetRequiredSection(CorsOptions.SectionName)
        .Get<CorsOptions>() ?? new CorsOptions();

      services.AddCors(cors =>
      {
        cors.AddPolicy(CorsOptions.PolicyName, policy =>
        {
          policy
            .WithOrigins(options.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
      });

      return services;
    }
  }
}
