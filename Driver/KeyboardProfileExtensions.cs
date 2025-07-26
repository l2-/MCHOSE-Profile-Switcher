namespace Driver;

public static class KeyboardProfileExtensions
{
    public static Command CreateSetLightCommand(this KeyboardProfile keyboardProfile, byte[] baseKeyboardConfig)
        => Packets.CreateSetLightCommand(baseKeyboardConfig, keyboardProfile.Light);
}
