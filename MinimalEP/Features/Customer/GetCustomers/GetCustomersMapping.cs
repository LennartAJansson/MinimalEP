namespace MinimalEP.Features.Customer.GetCustomers;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;

public static class GetCustomersMapping
{
  extension(Customer customer)
  {
    public GetCustomersItemResponse ToItemResponse()
    {
      return new GetCustomersItemResponse(customer.Id, customer.Name, customer.Email, customer.RowVersion);
    }
  }

  extension(PagedResult<Customer> customers)
  {
    public GetCustomersResponse ToResponse()
    {
      return new GetCustomersResponse(customers.Items.Select(c => c.ToItemResponse()).ToList(), customers.NextCursor);
    }
  }
}
