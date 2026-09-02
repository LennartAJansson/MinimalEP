namespace MinimalEP.Tests.Authorization;

using System.Net;
using System.Net.Http.Json;

using MinimalEP.Domain.Core;
using MinimalEP.Tests.Infrastructure;

[Collection(IntegrationCollection.Name)]
public sealed class AuthorizationMatrixTests(MinimalEpApplicationFactory factory)
{
  public static TheoryData<string, string?, HttpStatusCode> Cases => new()
  {
    { "/api/v1/employees", null, HttpStatusCode.Unauthorized },
    { "/api/v1/employees", Roles.User, HttpStatusCode.Forbidden },
    { "/api/v1/employees", Roles.Admin, HttpStatusCode.OK },
    { "/api/v1/employees", Roles.SuperAdmin, HttpStatusCode.OK },
    { "/api/v1/customers", Roles.User, HttpStatusCode.Forbidden },
    { "/api/v1/customers", Roles.Admin, HttpStatusCode.OK },
    { "/api/v1/customers", Roles.SuperAdmin, HttpStatusCode.OK },
    { "/api/v1/me", Roles.User, HttpStatusCode.NotFound }
  };

  public static TheoryData<HttpMethod, string> RestrictedWrites => new()
  {
    { HttpMethod.Post, "/api/v1/customers" },
    { HttpMethod.Put, $"/api/v1/customers/{Guid.NewGuid()}" },
    { HttpMethod.Delete, $"/api/v1/customers/{Guid.NewGuid()}" },
    { HttpMethod.Put, $"/api/v1/employees/{Guid.NewGuid()}" },
    { HttpMethod.Delete, $"/api/v1/employees/{Guid.NewGuid()}" },
    { HttpMethod.Post, "/api/v1/admin/employees" },
    { HttpMethod.Put, $"/api/v1/admin/users/{Guid.NewGuid()}/role" }
  };

  [Theory]
  [MemberData(nameof(Cases))]
  public async Task Endpoint_enforces_expected_access(string path, string? role, HttpStatusCode expectedStatus)
  {
    using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
    if (role is not null)
      client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);

    using var response = await client.GetAsync(path, CancellationToken.None);

    Assert.Equal(expectedStatus, response.StatusCode);
  }

  [Theory]
  [MemberData(nameof(RestrictedWrites))]
  public async Task Administrative_write_endpoints_forbid_plain_users(HttpMethod method, string path)
  {
    using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
    client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, Roles.User);
    using var request = new HttpRequestMessage(method, path);

    using var response = await client.SendAsync(request, CancellationToken.None);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Theory]
  [InlineData("/health/live")]
  [InlineData("/health/ready")]
  public async Task Health_endpoints_are_anonymous_and_healthy(string path)
  {
    using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

    using var response = await client.GetAsync(path, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task Created_customer_location_uses_the_named_versioned_resource_route()
  {
    using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
    client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, Roles.Admin);
    var id = Guid.NewGuid();

    using var response = await client.PostAsJsonAsync(
      "/api/v1/customers",
      new { Name = "Route customer", Email = $"route-{id:N}@example.test" },
      CancellationToken.None);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.Matches("^https://localhost/api/v1/customers/[0-9a-f-]+$", response.Headers.Location?.ToString());
  }

  [Fact]
  public async Task Angular_origin_can_preflight_the_login_endpoint()
  {
    using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
    using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
    request.Headers.Add("Origin", "http://localhost:4200");
    request.Headers.Add("Access-Control-Request-Method", "POST");
    request.Headers.Add("Access-Control-Request-Headers", "content-type");

    using var response = await client.SendAsync(request, CancellationToken.None);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    Assert.Equal("http://localhost:4200", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    Assert.Contains("POST", response.Headers.GetValues("Access-Control-Allow-Methods"));
  }
}
