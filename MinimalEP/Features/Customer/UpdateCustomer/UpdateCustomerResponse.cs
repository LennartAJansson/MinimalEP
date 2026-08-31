namespace MinimalEP.Features.Customer.UpdateCustomer;

public record UpdateCustomerResponse(Guid Id, string Name, string Email, byte[] RowVersion);
