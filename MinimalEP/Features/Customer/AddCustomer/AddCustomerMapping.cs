namespace MinimalEP.Features.Customer.AddCustomer;
using MinimalEP.Domain.Model;

public static class AddCustomerMapping
{
  extension(AddCustomerRequest request)
  {
    public Customer ToEntity(Guid? creatorId = null)
    {
      return new Customer
      {
        Name = request.Name,
        Email = request.Email,
        Created = DateTimeOffset.UtcNow,
        CreatedBy = creatorId
      };
    }
  }

  extension(Customer customer)
  {
    public AddCustomerResponse ToResponse()
    {
      return new AddCustomerResponse(customer.Id, customer.Name, customer.Email);
    }
  }
}