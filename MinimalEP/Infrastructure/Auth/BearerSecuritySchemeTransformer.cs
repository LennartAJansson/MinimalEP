namespace MinimalEP.Infrastructure.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

public sealed class BearerSecuritySchemeTransformer(
  IAuthenticationSchemeProvider schemeProvider)
  : IOpenApiDocumentTransformer
{
  public async Task TransformAsync(
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    var schemes = await schemeProvider.GetAllSchemesAsync();
    if (!schemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
      return;

    var bearerScheme = new OpenApiSecurityScheme
    {
      Type = SecuritySchemeType.Http,
      Scheme = "bearer",
      BearerFormat = "JWT",
      Description = "Ange din JWT-token (utan 'Bearer '-prefix)."
    };

    document.Components ??= new OpenApiComponents();
    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    document.Components.SecuritySchemes["Bearer"] = bearerScheme;

    var securityRequirement = new OpenApiSecurityRequirement
    {
      [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    };

    // Lägg på security-krav på alla operationer
    foreach (var path in document.Paths.Values)
      foreach (var operation in path.Operations.Values)
      {
        operation.Security ??= [];
        operation.Security.Add(securityRequirement);
      }
  }
}
