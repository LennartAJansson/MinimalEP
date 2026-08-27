# Scalar / OpenAPI med Bearer Auth

## Syfte
Exponera Scalar-portalen med Bearer JWT-stöd så att lås-ikonen fungerar och skyddade endpoints kan anropas direkt från UI:t.

## Problem: operation.Security är null
I Microsoft.OpenApi 2.7.5 är `OpenApiOperation.Security` **null by default**.
Utan initiering kraschar dokumenttransformatorn tyst.

## BearerSecuritySchemeTransformer
```csharp
public class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemeProvider)
	: IOpenApiDocumentTransformer
{
	public async Task TransformAsync(
		OpenApiDocument document,
		OpenApiDocumentTransformerContext context,
		CancellationToken ct)
	{
		var schemes = await schemeProvider.GetAllSchemesAsync();
		if (!schemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
			return;

		document.Components ??= new();
		document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
		document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
		{
			Type         = SecuritySchemeType.Http,
			Scheme       = "bearer",
			BearerFormat = "JWT"
		};

		foreach (var path in document.Paths.Values)
		{
			foreach (var operation in path.Operations.Values)
			{
				operation.Security ??= [];   // ← kritisk init, null annars
				operation.Security.Add(new OpenApiSecurityRequirement
				{
					[new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id   = "Bearer"
						}
					}] = []
				});
			}
		}
	}
}
```

## Registrering
```csharp
services.AddOpenApi(options =>
	options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
```

## Rätt namespace — Microsoft.OpenApi 2.7.5
```csharp
using Microsoft.OpenApi.Models;   // OpenApiDocument, OpenApiOperation, OpenApiSecurityScheme, etc.
using Microsoft.OpenApi;          // ReferenceType, SecuritySchemeType
```

## Scalar i Program.cs
```csharp
app.MapScalarApiReference(options =>
{
	options.WithTitle("MinimalEP API");
	options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
app.MapOpenApi();
```

## Viktigt
- `operation.Security ??= []` måste köras innan `.Add(...)` — annars NullReferenceException
- Scalar fungerar med HTTP om HTTPS-profilen ger problem i managed/corporate miljö
