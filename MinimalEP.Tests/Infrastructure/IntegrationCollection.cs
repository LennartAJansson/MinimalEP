namespace MinimalEP.Tests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestFixture : ICollectionFixture<MinimalEpApplicationFactory>
{
  public const string Name = "Integration";
}
