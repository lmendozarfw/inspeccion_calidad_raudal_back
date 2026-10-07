using System.Text.Json;
using System.Text.Json.Serialization;

namespace Calidad_API.Utilities;

public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.GetDateTime();
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        var utcValue = DateTime.SpecifyKind(
            value,
            DateTimeKind.Utc
        );

        writer.WriteStringValue(utcValue);
    }
}