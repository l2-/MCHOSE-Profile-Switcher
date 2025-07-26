using System.Text.Json;

namespace Common;

public static class DeserializeExtensions
{
    private static readonly JsonSerializerOptions DefaultSerializerSettings = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static T? Deserialize<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultSerializerSettings);
    }
}
