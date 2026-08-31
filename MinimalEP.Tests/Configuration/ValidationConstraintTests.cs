namespace MinimalEP.Tests.Configuration;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Customer.AddCustomer;
using MinimalEP.Features.Employee.Me;
using MinimalEP.Features.Workload.StartWorkload;

public sealed class ValidationConstraintTests
{
  [Fact]
  public async Task Customer_validator_rejects_values_beyond_database_lengths()
  {
    var request = new AddCustomerRequest(
      new string('a', CustomerConstraints.NameMaxLength + 1),
      $"{new string('a', CustomerConstraints.EmailMaxLength)}@example.test");

    var result = await new AddCustomerValidator().ValidateAsync(request);

    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task Employee_validator_rejects_values_beyond_database_lengths()
  {
    var request = new UpdateMeRequest(
      new string('a', EmployeeConstraints.NameMaxLength + 1),
      "Surname",
      EmployeeConstraints.MinimumAge,
      "Position",
      "Phone",
      "Street",
      "PostalCode",
      "City",
      [1]);

    var result = await new UpdateMeValidator().ValidateAsync(request);

    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task Workload_validator_rejects_comments_beyond_database_length()
  {
    var request = new StartWorkloadRequest(
      Guid.CreateVersion7(),
      DateTimeOffset.UtcNow,
      new string('a', WorkloadConstraints.CommentsMaxLength + 1));

    var result = await new StartWorkloadValidator().ValidateAsync(request);

    Assert.False(result.IsValid);
  }
}
