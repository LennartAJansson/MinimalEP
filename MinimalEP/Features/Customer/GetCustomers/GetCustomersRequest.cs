namespace MinimalEP.Features.Customer.GetCustomers;

public record GetCustomersRequest(int? PageSize, Guid? After);
