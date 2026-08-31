namespace MinimalEP.Features.Customer.GetCustomer;

using MinimalEP.Domain.Model;

public static class GetCustomerMapping
{
  extension(Customer customer)
  {
    public GetCustomerResponse ToResponse()
    {
      return new GetCustomerResponse(customer.Id, customer.Name, customer.Email, customer.RowVersion);
    }
  }
}
