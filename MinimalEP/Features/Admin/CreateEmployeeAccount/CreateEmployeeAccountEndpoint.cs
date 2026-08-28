namespace MinimalEP.Features.Admin.CreateEmployeeAccount;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class CreateEmployeeAccountEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost("/admin/employees", async (
      CreateEmployeeAccountRequest request,
      IRequestHandler<CreateEmployeeAccountRequest, Result<CreateEmployeeAccountResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      IResult httpResult = result switch
      {
        Result<CreateEmployeeAccountResponse>.Ok ok      => TypedResults.Created($"/employees/{ok.Value.UserId}", ok.Value),
        Result<CreateEmployeeAccountResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                                => throw new UnreachableException()
      };

      return httpResult;
    }).RequireAuthorization("AdminOrAbove");
  }
}
