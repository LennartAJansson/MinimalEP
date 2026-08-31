namespace MinimalEP.Features.Workload.GetWorkload;

using MinimalEP.Domain.Model;

public static class GetWorkloadMapping
{
  extension(Workload workload)
  {
    public GetWorkloadResponse ToResponse()
    {
      return new GetWorkloadResponse(
        workload.Id,
        workload.CustomerId,
        workload.Customer.Name,
        workload.EmployeeId,
        workload.Employee.Name,
        workload.Start,
        workload.Stop,
        workload.Comments,
        workload.RowVersion);
    }
  }
}
