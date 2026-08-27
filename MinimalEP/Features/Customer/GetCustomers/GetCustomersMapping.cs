namespace MinimalEP.Features.Customer.GetCustomers;

using MinimalEP.Domain.Model;

public static class GetCustomersMapping
{
  extension(Customer customer)
  {
    public GetCustomersItemResponse ToItemResponse()
    {
      return new GetCustomersItemResponse(customer.Id, customer.Name, customer.Email);
    }
  }

  extension(IReadOnlyList<Customer> customers)
  {
    public GetCustomersResponse ToResponse()
    {
      return new GetCustomersResponse(customers.Select(c => c.ToItemResponse()).ToList());
    }
  }
}
