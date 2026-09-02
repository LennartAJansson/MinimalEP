using Asp.Versioning;
using Asp.Versioning.Builder;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using Scalar.AspNetCore;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;
using MinimalEP.Infrastructure.Cors;
using MinimalEP.Infrastructure.Data.Core;
using MinimalEP.Infrastructure.Observability;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();

builder.Services
  .AddApiVersioning(options =>
  {
    options.DefaultApiVersion = new ApiVersion(ApiVersions.V1);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
  })
  .AddApiExplorer(options =>
  {
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
  })
  .AddOpenApi();

builder.Services
  .AddEndpoints(typeof(Program))
  .AddApplicationCors(builder.Configuration)
  .AddApplicationData(builder.Configuration)
  .AddApplicationAuth(builder.Configuration)
  .AddJwtOpenApi()
  .AddApplicationObservability(builder.Configuration);

var app = builder.Build();

// Creates the database and applies any pending migrations automatically on startup —
// no manual `dotnet ef database update` needed for local/demo scenarios.
await RoleSeeder.MigrateDatabaseAsync(app.Services);
await RoleSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi().WithDocumentPerVersion();
  app.MapScalarApiReference(options =>
  {
    options.WithTitle("MinimalEP API")
           .WithTheme(ScalarTheme.DeepSpace)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
  });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(CorsOptions.PolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks(ApiRoutes.LiveHealth, new HealthCheckOptions
{
  Predicate = _ => false
}).AllowAnonymous();
app.MapHealthChecks(ApiRoutes.ReadyHealth, new HealthCheckOptions
{
  Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(ApiVersions.V1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup(ApiRoutes.VersionedGroup)
    .WithApiVersionSet(apiVersionSet)
    .RequireAuthorization();

// Auth routes override authorization via AllowAnonymous on each endpoint
app.MapEndpoints(versionedGroup);

app.Run();

public partial class Program;
