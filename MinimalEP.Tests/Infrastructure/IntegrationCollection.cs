namespace MinimalEP.Tests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationCollection : ICollectionFixture<MinimalEpApplicationFactory>
{
  public const string Name = "Integration";
}
