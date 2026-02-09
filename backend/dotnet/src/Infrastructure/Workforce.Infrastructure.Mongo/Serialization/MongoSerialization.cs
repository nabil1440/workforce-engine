using MongoDB.Bson.Serialization;

namespace Workforce.Infrastructure.Mongo.Serialization;

public static class MongoSerialization
{
    private static bool _initialized;
    private static readonly object Sync = new();

    public static void Register()
    {
        if (_initialized)
        {
            return;
        }

        lock (Sync)
        {
            if (_initialized)
            {
                return;
            }

            BsonSerializer.RegisterSerializer(new DateOnlySerializer());

            _initialized = true;
        }
    }
}
