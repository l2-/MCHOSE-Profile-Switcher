namespace Driver;

public static class KeyboardProfileExtensions
{
    public static Command CreateSetLightCommand(this KeyboardProfile keyboardProfile, byte[] baseKeyboardConfig)
        => Packets.CreateSetLightCommand(baseKeyboardConfig, keyboardProfile.Light);

    public static Command CreateSetAllUserKeys(this KeyboardProfile keyboardProfile, int layer)
        => Packets.CreateSetAllUserKeys(keyboardProfile.Userkeys, layer);

    public static Command CreateSetTravelKeys(this KeyboardProfile keyboardProfile)
        => Packets.CreateSetAllTravelKeys(keyboardProfile.TravelKeys);

    public static Command CreateSetMacro(this KeyboardProfile keyboardProfile)
        => Packets.CreateSetMacro(keyboardProfile.Macro);

    public static Command CreateSetAllTglKeyInfo(this KeyboardProfile keyboardProfile)
        => Packets.CreateSetAllTglKeyInfo([..keyboardProfile.AdvancedKeys
            .Where(advancedKey => advancedKey.Type.Equals("tgl", StringComparison.CurrentCultureIgnoreCase))
            .Select(advancedKey => advancedKey.TglKey)
            .OfType<Tglkey>()]);

    public static Command CreateSetAllMtKeyInfo(this KeyboardProfile keyboardProfile)
        => Packets.CreateSetAllMtKeyInfo([.. keyboardProfile.AdvancedKeys
            .SelectMany<AdvancedKey, (AdvancedKeySimpleSettings, AdvancedKeySimpleSettings)>(advancedKey => {
                if (advancedKey.Type.Equals("mt", StringComparison.InvariantCultureIgnoreCase)
                    && advancedKey.MtClickKey is { } click
                    && advancedKey.MtDownKey is { } down)
                {
                    return [(click, down)];
                }
                if (advancedKey.Type.ToLowerInvariant() is "socd" or "rs" or "oks"
                    && advancedKey.Key1 is { } key1
                    && advancedKey.Key2 is { } key2)
                {
                    var simpleKey1 = new AdvancedKeySimpleSettings {
                        Type = key1.Type,
                        Code1 = key1.Code1,
                        Code2 = key1.Code2,
                    };
                    var simpleKey2 = new AdvancedKeySimpleSettings {
                        Type = key2.Type,
                        Code1 = key2.Code1,
                        Code2 = key2.Code2,
                    };
                    return [(simpleKey1, simpleKey2), (simpleKey2, simpleKey1)];
                }
                return [];
            })
            .OfType<(AdvancedKeySimpleSettings, AdvancedKeySimpleSettings)>()]);

    public static Command CreateSetAllDksKeyInfo(this KeyboardProfile keyboardProfile)
        => Packets.CreateSetAllDksKeyInfo([.. keyboardProfile.AdvancedKeys
            .Where(advancedKey => advancedKey.Type.Equals("dks", StringComparison.InvariantCultureIgnoreCase))
            .Select<AdvancedKey, (IReadOnlyCollection<int>?, IReadOnlyCollection<DksKey>)?>(
                advancedKey => advancedKey.DksKeys is { } dksKeys
                ? (advancedKey.DksPoint, advancedKey.DksKeys)
                : null)
            .OfType<(IReadOnlyCollection<int>?, IReadOnlyCollection<DksKey>)>()]);
}
