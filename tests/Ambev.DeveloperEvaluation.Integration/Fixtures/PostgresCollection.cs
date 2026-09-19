using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Fixtures;

[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
