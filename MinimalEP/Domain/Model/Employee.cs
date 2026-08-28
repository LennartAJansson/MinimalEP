namespace MinimalEP.Domain.Model;

public class Employee : BaseEntity
{
  public required string Email { get; set; }
  public required string GivenName { get; set; }
  public required string Surname { get; set; }
  public required int Age { get; set; }
  public required string Position { get; set; }
  public required string PhoneNumber { get; set; }
  public required Address Address { get; set; }

  // Computed, not persisted — kept for convenience in mappings/JWT claims so callers
  // don't need to know the underlying GivenName/Surname split.
  public string Name => $"{GivenName} {Surname}";
}
