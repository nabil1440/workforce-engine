namespace Workforce.Infrastructure.Mongo.Options;

public sealed class MongoOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = "workforce";
}
