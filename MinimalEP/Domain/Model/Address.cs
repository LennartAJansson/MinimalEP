namespace MinimalEP.Domain.Model;

// Owned type (EF Core "owned entity"): Address has no identity of its own — it only exists
// as part of an Employee. This models a common DDD value-object pattern: equality by value,
// no separate table/repository, persisted as columns on the owner's table.
public class Address
{
  public required string Street { get; set; }
  public required string PostalCode { get; set; }
  public required string City { get; set; }
}
