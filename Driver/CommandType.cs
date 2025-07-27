namespace Driver;

public enum CommandType
{
    Info = 3,
    Base = 4,
    GetBasePacket = 5,
    Lighting = 6,
    SetAllUserKeys = 9,
    SetMacro = 13,
    SetTravelKeys = 161,
    SetDksKeyInfo = 163,
    SetMtKeyInfo = 165,
    SetTglKeyInfo = 167,
}

public static class CommandTypeExtensions
{
    public static byte As(this CommandType commandType)
        => (byte)commandType;
}
