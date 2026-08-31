namespace MinimalEP.Features.Workload.GetWorkloads;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;

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
        workload.Comments,
        workload.RowVersion);
    }
  }

  extension(PagedResult<Workload> workloads)
  {
    public GetWorkloadsResponse ToResponse()
    {
      return new GetWorkloadsResponse(workloads.Items.Select(w => w.ToItemResponse()).ToList(), workloads.NextCursor);
    }
  }
}
