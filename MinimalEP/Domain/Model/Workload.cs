namespace MinimalEP.Domain.Model;

public class Workload : BaseEntity
{
  public required Guid CustomerId { get; set; }
  public required Guid EmployeeId { get; set; }
  public required DateTimeOffset Start { get; set; }
  public DateTimeOffset? Stop { get; set; }
  public string? Comments { get; set; }

  public Customer Customer { get; set; } = null!;
  public Employee Employee { get; set; } = null!;
}
