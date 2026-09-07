namespace MinimalEP.Tests.Domain;

using MinimalEP.Domain.Model;

public sealed class WorkloadTransitionTests
{
  [Fact]
  public void StartNew_creates_an_open_workload()
  {
    var customerId = Guid.CreateVersion7();
    var employeeId = Guid.CreateVersion7();
    var start = DateTimeOffset.UtcNow;

    var workload = Workload.StartNew(customerId, employeeId, start, "Started");

    Assert.Equal(customerId, workload.CustomerId);
    Assert.Equal(employeeId, workload.EmployeeId);
    Assert.Equal(start, workload.Start);
    Assert.Equal("Started", workload.Comments);
    Assert.Null(workload.Stop);
  }

  [Fact]
  public void StopAt_throws_when_workload_is_already_stopped()
  {
    var start = DateTimeOffset.UtcNow.AddHours(-2);
    var workload = Workload.StartNew(Guid.CreateVersion7(), Guid.CreateVersion7(), start, null);
    workload.StopAt(start.AddHours(1));

    var exception = Assert.Throws<InvalidOperationException>(() => workload.StopAt(start.AddHours(2)));

    Assert.Equal("Workload is already stopped.", exception.Message);
  }

  [Fact]
  public void UpdateDetails_throws_when_start_is_not_before_stop()
  {
    var start = DateTimeOffset.UtcNow.AddHours(-2);
    var workload = Workload.StartNew(Guid.CreateVersion7(), Guid.CreateVersion7(), start, null);
    workload.StopAt(start.AddHours(1));

    var exception = Assert.Throws<InvalidOperationException>(() => workload.UpdateDetails(start.AddHours(1), "Invalid"));

    Assert.Equal("Start time must be before Stop.", exception.Message);
  }
}
