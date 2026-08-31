using Microsoft.VSDiagnostics;

namespace MinimalEP.Benchmarks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalEP.Domain.Model;
using MinimalEP.Infrastructure.Data.Context;
using MinimalEP.Infrastructure.Data.Core;

[SimpleJob(RuntimeMoniker.Net10_0, warmupCount: 1, iterationCount: 3)]
[CPUUsageDiagnoser]
public class RepositoryReadBenchmarks
{
    private const string DatabaseName = "MinimalEP_Benchmarks";
    private string connectionString = null!;
    private ApplicationDbContext context = null!;
    private CustomerRepository customerRepository = null!;
    private EmployeeRepository employeeRepository = null!;
    private WorkloadRepository workloadRepository = null!;
    private Guid historyEmployeeId;
    [GlobalSetup]
    public async Task Setup()
    {
        connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options;
        context = new ApplicationDbContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
        var customers = Enumerable.Range(0, 1_000).Select(i => new Customer { Name = $"Customer {i}", Email = $"customer-{i}@benchmark.test" }).ToArray();
        var employees = Enumerable.Range(0, 1_000).Select(i => new Employee { Email = $"employee-{i}@benchmark.test", GivenName = "Benchmark", Surname = $"Employee {i}", Age = 30, Position = "Consultant", PhoneNumber = "0000000000", Address = new Address { Street = "Street", PostalCode = "00000", City = "City" } }).ToArray();
        context.AddRange(customers);
        context.AddRange(employees);
        await context.SaveChangesAsync();
        historyEmployeeId = employees[0].Id;
        var workloads = Enumerable.Range(0, 10_000).Select(i => new Workload { CustomerId = customers[i % customers.Length].Id, EmployeeId = historyEmployeeId, Start = DateTimeOffset.UtcNow.AddHours(-i - 1), Stop = DateTimeOffset.UtcNow.AddHours(-i), Comments = "Benchmark" });
        context.Workloads.AddRange(workloads);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = connectionString }).Build();
        var connectionFactory = new SqlConnectionFactory(configuration);
        customerRepository = new CustomerRepository(context, connectionFactory);
        employeeRepository = new EmployeeRepository(context, connectionFactory);
        workloadRepository = new WorkloadRepository(context, connectionFactory);
    }

    [Benchmark]
    public Task<IReadOnlyList<Customer>> GetAllCustomers() => customerRepository.GetAllAsync(CancellationToken.None);
    [Benchmark]
    public Task<IReadOnlyList<Employee>> GetAllEmployees() => employeeRepository.GetAllAsync(CancellationToken.None);
    [Benchmark]
    public Task<IReadOnlyList<Workload>> GetAllWorkloads() => workloadRepository.GetAllAsync(CancellationToken.None);
    [Benchmark]
    public Task<IReadOnlyList<Workload>> GetEmployeeWorkloadHistory() => workloadRepository.GetByEmployeeAsync(historyEmployeeId, CancellationToken.None);
    [GlobalCleanup]
    public async Task Cleanup()
    {
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }
}