namespace MinimalEP.Domain.Model;

public class BaseEntity
{
  public Guid Id { get; set; } = Guid.CreateVersion7();
  public DateTimeOffset? Created { get; set; }
  public DateTimeOffset? Updated { get; set; }
  public DateTimeOffset? Deleted { get; set; }
  public Guid? CreatedBy { get; set; }
  public Guid? UpdatedBy { get; set; }
  public Guid? DeletedBy { get; set; }
}