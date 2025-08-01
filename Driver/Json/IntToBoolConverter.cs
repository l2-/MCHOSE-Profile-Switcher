using System.Text.Json;
using System.Text.Json.Serialization;

namespace Driver.Json;

public class IntToBoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TryGetInt32(out var @int))
            {
                return @int switch
                {
                    0 => false,
                    1 => true,
                    _ => throw new JsonException(),
                };
            }
        }
        catch { }
        try
        {
            return reader.GetString()?.ToLowerInvariant() switch
            {
                "0" => false,
                "1" => true,
                "false" => false,
                "true" => true,
                _ => throw new JsonException(),
            };
        }
        catch { }

        try
        {
            return reader.GetBoolean();
        }
        catch { }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteBooleanValue(value);
}
