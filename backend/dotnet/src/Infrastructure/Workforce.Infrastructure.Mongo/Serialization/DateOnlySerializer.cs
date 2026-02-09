using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Workforce.Infrastructure.Mongo.Serialization;

public sealed class DateOnlySerializer : StructSerializerBase<DateOnly>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
    {
        var dateTime = value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        context.Writer.WriteDateTime(BsonUtils.ToMillisecondsSinceEpoch(dateTime));
    }

    public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();

        if (bsonType == BsonType.DateTime)
        {
            var milliseconds = context.Reader.ReadDateTime();
            var dateTime = BsonUtils.ToDateTimeFromMillisecondsSinceEpoch(milliseconds);
            return DateOnly.FromDateTime(dateTime);
        }

        if (bsonType == BsonType.String)
        {
            var value = context.Reader.ReadString();
            return DateOnly.Parse(value, CultureInfo.InvariantCulture);
        }

        context.Reader.SkipValue();
        return default;
    }
}
