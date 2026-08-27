namespace MinimalEP.Domain.Model;

public class Employee : BaseEntity
{
  public required string Name { get; set; }
  public required int Age { get; set; }
  public required string Position { get; set; }
}
