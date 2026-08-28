using Asp.Versioning;
using Asp.Versioning.Builder;

using Scalar.AspNetCore;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;
using MinimalEP.Infrastructure.Data.Core;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services
  .AddApiVersioning(options =>
  {
    options.DefaultApiVersion = new ApiVersion(1);
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
  .AddApplicationData(builder.Configuration)
  .AddApplicationAuth(builder.Configuration)
  .AddJwtOpenApi();

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet)
    .RequireAuthorization();

// Auth routes override authorization via AllowAnonymous on each endpoint
app.MapEndpoints(versionedGroup);

app.Run();
