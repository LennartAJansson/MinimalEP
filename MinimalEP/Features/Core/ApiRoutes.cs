namespace MinimalEP.Features.Core;

public static class ApiRoutes
{
  public const string VersionedGroup = "api/v{version:apiVersion}";
  public const string LiveHealth = "/health/live";
  public const string ReadyHealth = "/health/ready";

  public static class Auth
  {
    public const string Register = "/auth/register";
    public const string Login = "/auth/login";
    public const string Refresh = "/auth/refresh";
  }

  public static class Admin
  {
    public const string Employees = "/admin/employees";
    public const string UserRole = "/admin/users/{userId}/role";
  }

  public static class Customers
  {
    public const string Collection = "/customers";
    public const string ById = "/customers/{id}";
  }

  public static class Employees
  {
    public const string Collection = "/employees";
    public const string ById = "/employees/{id}";
    public const string Me = "/me";
  }

  public static class Workloads
  {
    public const string Collection = "/workloads";
    public const string ById = "/workloads/{id}";
    public const string Start = "/workloads/start";
    public const string Stop = "/workloads/{id}/stop";
  }
}

public static class ApiRouteNames
{
  public const string GetCustomer = nameof(GetCustomer);
  public const string GetEmployee = nameof(GetEmployee);
  public const string GetMe = nameof(GetMe);
  public const string GetWorkload = nameof(GetWorkload);
}

public static class ApiVersions
{
  public const int V1 = 1;
  public const string V1RouteValue = "1";
}
