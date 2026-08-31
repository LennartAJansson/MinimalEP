namespace MinimalEP.Features.Customer.GetCustomer;

public record GetCustomerResponse(Guid Id, string Name, string Email, byte[] RowVersion);
