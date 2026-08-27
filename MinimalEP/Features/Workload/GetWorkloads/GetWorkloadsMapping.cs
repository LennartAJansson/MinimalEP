namespace MinimalEP.Features.Workload.GetWorkloads;

using MinimalEP.Domain.Model;

public static class GetWorkloadsMapping
{
  extension(Workload workload)
  {
    public GetWorkloadsItemResponse ToItemResponse()
    {
      return new GetWorkloadsItemResponse(
        workload.Id,
        workload.CustomerId,
        workload.Customer.Name,
        workload.EmployeeId,
        workload.Employee.Name,
        workload.Start,
        workload.Stop,
        workload.Comments);
    }
  }

  extension(IReadOnlyList<Workload> workloads)
  {
    public GetWorkloadsResponse ToResponse()
    {
      return new GetWorkloadsResponse(workloads.Select(w => w.ToItemResponse()).ToList());
    }
  }
}
