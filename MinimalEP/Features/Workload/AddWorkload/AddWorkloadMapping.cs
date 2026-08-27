namespace MinimalEP.Features.Workload.AddWorkload;

using MinimalEP.Domain.Model;

public static class AddWorkloadMapping
{
  extension(AddWorkloadRequest request)
  {
    // EmployeeId sätts av AuditAndSoftDeleteInterceptor från JWT-claimet
    public Workload ToEntity(Guid employeeId)
    {
      return new Workload
      {
        CustomerId = request.CustomerId,
        EmployeeId = employeeId,
        Start = request.Start,
        Stop = request.Stop,
        Comments = request.Comments
      };
    }
  }

  extension(Workload workload)
  {
    public AddWorkloadResponse ToResponse()
    {
      return new AddWorkloadResponse(
        workload.Id,
        workload.CustomerId,
        workload.EmployeeId,
        workload.Start,
        workload.Stop,
        workload.Comments);
    }
  }
}
