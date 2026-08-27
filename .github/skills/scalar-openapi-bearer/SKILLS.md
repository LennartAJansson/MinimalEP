# Scalar / OpenAPI with Bearer Auth

## Purpose
Expose the Scalar portal with Bearer JWT support so the lock icon works and protected endpoints can be called directly from the UI.

## Known issues — Microsoft.OpenApi 2.7.5

| Issue | Fix |
|-------|-----|
| `OpenApiOperation.Security` is **null** by default | `operation.Security ??= []` before `.Add(...)` |
| `document.Paths` can be null | Use `if (document.Paths is not { } paths) return;` to assign to local variable — compiler flow analysis does not track null-state through `if (x is null) return` in async methods |
| `path.Operations` can be null | `if (path.Operations is null) continue;` |

## BearerSecuritySchemeTransformer — correct implementation
```csharp
public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemeProvider)
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
			Type         = SecuritySchemeType.Http,
			Scheme       = "bearer",
			BearerFormat = "JWT"
		};

		document.Components ??= new OpenApiComponents();
		document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
		document.Components.SecuritySchemes["Bearer"] = bearerScheme;

		var securityRequirement = new OpenApiSecurityRequirement
		{
			[new OpenApiSecuritySchemeReference("Bearer", document)] = []
		};

		// Assign to local — required for nullable flow analysis in async methods
		if (document.Paths is not { } paths)
			return;

		foreach (var path in paths.Values)
		{
			if (path.Operations is null)
				continue;

			foreach (var operation in path.Operations.Values)
			{
				operation.Security ??= [];
				operation.Security.Add(securityRequirement);
			}
		}
	}
}
```

## Registration — use ConfigureAll, not AddOpenApi
When `AddApiVersioning().AddOpenApi()` is already called (required by AV0029), do **not** call `services.AddOpenApi(...)` again to attach the transformer — it registers a duplicate document. Use `ConfigureAll` instead:

```csharp
public IServiceCollection AddJwtOpenApi()
{
	services.ConfigureAll<OpenApiOptions>(options =>
	{
		options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
	});
	return services;
}
```

## AV0029 — false positive in generated file
The OpenAPI source generator reports AV0029 on its own generated file when `Asp.Versioning.OpenApi` is used. This is not an error in your code. Suppress it in `.csproj`:

```xml
<NoWarn>$(NoWarn);AV0029</NoWarn>
```

## AV0030 — document per API version
```csharp
app.MapOpenApi().WithDocumentPerVersion();
```

## Correct namespace — Microsoft.OpenApi 2.7.5
```csharp
using Microsoft.OpenApi.Models;   // OpenApiDocument, OpenApiComponents, etc.
using Microsoft.OpenApi;          // SecuritySchemeType, ReferenceType
```

## Scalar in Program.cs
```csharp
app.MapScalarApiReference(options =>
{
	options.WithTitle("My API")
		   .WithTheme(ScalarTheme.DeepSpace)
		   .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
app.MapOpenApi().WithDocumentPerVersion();
```

## Notes
- Scalar works with HTTP if the HTTPS profile causes issues in managed/corporate environments
