namespace MinimalEP.Domain.Model;

public class Workload : BaseEntity
{
  public required Guid CustomerId { get; set; }
  public required Guid EmployeeId { get; set; }
  public DateTimeOffset Start { get; private set; }
  public DateTimeOffset? Stop { get; private set; }
  public string? Comments { get; private set; }
  public byte[] RowVersion { get; set; } = [];

  public Customer Customer { get; set; } = null!;
  public Employee Employee { get; set; } = null!;

  public static Workload StartNew(Guid customerId, Guid employeeId, DateTimeOffset start, string? comments)
  {
    var workload = new Workload
    {
      CustomerId = customerId,
      EmployeeId = employeeId,
      Start = start
    };

    workload.UpdateDetails(start, comments);

    return workload;
  }

  public void StopAt(DateTimeOffset stop)
  {
    if (Stop.HasValue)
      throw new InvalidOperationException("Workload is already stopped.");

    if (stop <= Start)
      throw new InvalidOperationException("Stop time must be after Start.");

    Stop = stop;
  }

  public void UpdateDetails(DateTimeOffset start, string? comments)
  {
    if (Stop.HasValue && start >= Stop.Value)
      throw new InvalidOperationException("Start time must be before Stop.");

    Start = start;
    Comments = comments;
  }
}
