namespace MinimalEP.Features.Workload.StartWorkload;

using MinimalEP.Domain.Model;

public static class StartWorkloadMapping
{
  extension(StartWorkloadRequest request)
  {
    // EmployeeId comes from IUserContext, never from client input (OWASP API1: broken
    // object level authorization / "never trust client-supplied identity").
    public Workload ToEntity(Guid employeeId)
    {
      return new Workload
      {
        CustomerId = request.CustomerId,
        EmployeeId = employeeId,
        Start = request.Start,
        Comments = request.Comments
      };
    }
  }

  extension(Workload workload)
  {
    public StartWorkloadResponse ToResponse()
    {
      return new StartWorkloadResponse(
        workload.Id,
        workload.CustomerId,
        workload.EmployeeId,
        workload.Start,
        workload.Comments);
    }
  }
}
