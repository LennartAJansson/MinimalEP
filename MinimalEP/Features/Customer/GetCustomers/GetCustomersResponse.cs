namespace MinimalEP.Features.Customer.GetCustomers;

public record GetCustomersResponse(IReadOnlyList<GetCustomersItemResponse> Items);

public record GetCustomersItemResponse(Guid Id, string Name, string Email);
