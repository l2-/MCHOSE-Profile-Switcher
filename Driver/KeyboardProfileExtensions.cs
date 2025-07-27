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
        => Packets.CreateSetAllTglKeyInfo(keyboardProfile.AdvancedKeys);
}
