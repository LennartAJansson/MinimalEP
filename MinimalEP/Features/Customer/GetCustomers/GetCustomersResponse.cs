namespace MinimalEP.Features.Customer.GetCustomers;

public record GetCustomersResponse(IReadOnlyList<GetCustomersItemResponse> Items, Guid? NextCursor);

public record GetCustomersItemResponse(Guid Id, string Name, string Email, byte[] RowVersion);
