namespace MinimalEP.Domain.Model;

public class Customer
  : BaseEntity
{
  public required string Name { get; set; }
  public required string Email { get; set; }
}
