namespace Driver;

public sealed record KeyboardSpecs
{
    public required Info Info { get; init; }

    public required Base Base { get; init; }
}
