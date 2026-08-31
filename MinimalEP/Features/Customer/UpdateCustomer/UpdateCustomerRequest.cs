namespace MinimalEP.Features.Customer.UpdateCustomer;

public record UpdateCustomerRequest(Guid Id, string Name, string Email, byte[] RowVersion);
