using System.Text.Json.Serialization;

namespace Driver;

public sealed record KeyboardProfile
{
    public required Detail Detail { get; set; }
    public required UserKeys Userkeys { get; set; }
    public required IReadOnlyList<TravelKey> TravelKeys { get; set; }
    public required Light Light { get; set; }
    public required Globaltravel GlobalTravel { get; set; }
    public required int ReportRate { get; set; }
    public required Dictionary<string, IReadOnlyList<object>> ColorKeys { get; set; }
    // Not supported
    public required IReadOnlyList<object> AdvancedKeys { get; set; }
    public required IReadOnlyList<Macro> Macro { get; set; }
}

public sealed record Detail
{
    public required string Name { get; set; }
}

public sealed record UserKeys
{
    [JsonPropertyName("0")]
    public required IReadOnlyList<UserKey> Layer1 { get; set; }
    [JsonPropertyName("1")]
    public required IReadOnlyList<UserKey> Layer2 { get; set; }
    [JsonPropertyName("2")]
    public required IReadOnlyList<UserKey> Layer3 { get; set; }
    [JsonPropertyName("3")]
    public required IReadOnlyList<UserKey> Layer4 { get; set; }
}

public sealed record UserKey
{
    public required int Type { get; set; }
    public required int Code1 { get; set; }
    public required int Code2 { get; set; }
    public int? Code { get; set; }
    public string? Name { get; set; }
    public int? Index { get; set; }
    public int? Profile { get; set; }
    public int? Layer { get; set; }
}

public sealed record Light
{
    public required int Type { get; set; }
    public required int Effect { get; set; }
    public required int Brightness { get; set; }
    public required int Speed { get; set; }
    [JsonPropertyName("direct")]
    public required int Direction { get; set; }
    public required int SingleColor { get; set; }
    public required int ColorIndex { get; set; }
    [JsonPropertyName("h")]
    public required int Hue { get; set; }
    [JsonPropertyName("s")]
    public required int Saturation { get; set; }
    [JsonPropertyName("v")]
    public required int Value { get; set; }
    public required string HexColor { get; set; }
    [JsonPropertyName("side_effect")]
    public required int SideEffect { get; set; }
    [JsonPropertyName("side_brightness")]
    public required int SideBrightness { get; set; }
    [JsonPropertyName("side_speed")]
    public required int SideSpeed { get; set; }
    [JsonPropertyName("side_singleColor")]
    public required int SideSingleColor { get; set; }
    [JsonPropertyName("side_hexColor")]
    public required string SideHexColor { get; set; }
}

public sealed record Globaltravel
{
    public required int Actuation { get; set; }
    public required int PressDeadzone { get; set; }
    public required int ReleaseDeadzone { get; set; }
}

public sealed record TravelKey
{
    public required int SwitchType { get; set; }
    public required int Keymode { get; set; }
    public required int Isolated { get; set; }
    public required int Priority { get; set; }
    [JsonPropertyName("key_max_length")]
    public required int KeyMaxStrength { get; set; }
    public required int KeyActuation { get; set; }
    public required int PressPrecision { get; set; }
    public required int ReleasePrecision { get; set; }
    public required int RtPress { get; set; }
    public required int RtRelease { get; set; }
    public required int PressDeadzone { get; set; }
    public required int ReleaseDeadzone { get; set; }
    public required bool DeadzoneStatus { get; set; }
}

public sealed record MacroAction
{
    public required string Type { get; set; }
    public int? Code { get; set; }
    public string? Action { get; set; }
    public string? WebCode { get; set; }
    public int? Delay { get; set; }
}

public sealed record Macro
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required int Type { get; set; }
    // Not supported
    public required IReadOnlyList<MacroAction> MacroActions { get; set; }
}
