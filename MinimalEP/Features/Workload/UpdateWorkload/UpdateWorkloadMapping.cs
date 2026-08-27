namespace MinimalEP.Features.Workload.UpdateWorkload;

using MinimalEP.Domain.Model;

public static class UpdateWorkloadMapping
{
  extension(UpdateWorkloadRequest request)
  {
    public void ApplyTo(Workload workload)
    {
      workload.Start = request.Start;
      workload.Stop = request.Stop;
      workload.Comments = request.Comments;
    }
  }

  extension(Workload workload)
  {
    public UpdateWorkloadResponse ToResponse()
    {
      return new UpdateWorkloadResponse(
        workload.Id,
        workload.CustomerId,
        workload.EmployeeId,
        workload.Start,
        workload.Stop,
        workload.Comments);
    }
  }
}
