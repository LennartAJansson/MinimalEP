namespace MinimalEP.Features.Customer.UpdateCustomer;

using MinimalEP.Domain.Model;

public static class UpdateCustomerMapping
{
  extension(UpdateCustomerRequest request)
  {
    public void ApplyTo(Customer customer)
    {
      customer.Name = request.Name;
      customer.Email = request.Email;
    }
  }

  extension(Customer customer)
  {
    public UpdateCustomerResponse ToResponse()
    {
      return new UpdateCustomerResponse(customer.Id, customer.Name, customer.Email);
    }
  }
}
