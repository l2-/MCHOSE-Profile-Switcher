using Common;

namespace Driver;

public sealed record Command
{
    public required IReadOnlyCollection<byte[]> Packets { get; set; }
}

public static class Packets
{
    // Seems to always be 0 on my keyboard. Likely because it doesn;t support storing multiple profiles onboard.
    const int INDEX = 0;
    const int WRITE_PACKET_SIZE = 64;
    const int PAYLOAD_SIZE = 56;
    const int PROFILE_INDEX_SHIFT = 64;
    const byte PACKET_CONST = 85;
    public static readonly Command InfoCommand = new() { Packets = [Raw(PACKET_CONST, CommandType.Info.As(), 0, 32, 32)] };
    public static readonly Command BaseCommand = new() { Packets = [Raw(PACKET_CONST, CommandType.Base.As(), 0, 32, 32)] };

    public static byte Sum(this IEnumerable<byte> command)
        => (byte)(command.Sum(s => s) & 255);

    public static (byte s, byte l) SplitToByte(int e)
        => ((byte)(255 & e), (byte)(e >> 8));

    public static byte[] Prefix(byte commandType, int sliceIndex, int shiftedProfileIndex, byte[] command)
    {
        var length = (byte)command.Length;
        var (s, l) = SplitToByte(sliceIndex + shiftedProfileIndex);
        var sum = command.Concat([length, s, l]).Sum();
        return [PACKET_CONST, commandType, 0, sum, length, s, l, 0];
    }

    public static byte[] RightPad(byte[] arr, int totalSize, byte value = 0)
        => [.. arr, .. Enumerable.Repeat(value, totalSize - arr.Length)];

    public static byte[] Raw(byte type, params byte[] command)
        => RightPad([type, .. command], WRITE_PACKET_SIZE);

    public static IReadOnlyCollection<byte[]> ToPackets(byte commandType, int shiftedProfileIndex, byte[] command)
    {
        var packets = new List<byte[]>();
        for (int i = 0; i < command.Length;)
        {
            var sliceLength = Math.Min((byte)command.Length - i, PAYLOAD_SIZE);
            var slice = command[i..(i + sliceLength)];
            var prefix = Prefix(commandType, i, shiftedProfileIndex, slice);
            byte[] packet = [.. prefix, .. slice];
            packets.Add(packet);
            i += sliceLength;
        }
        return packets;
    }

    // getFuncConfig
    public static Command CreateGetKeyboardBasePacket(int profileIndex = INDEX)
    {
        var (a, i) = SplitToByte(PROFILE_INDEX_SHIFT * profileIndex);
        return new() { Packets = [RightPad([85, 5, 0, Sum([a, i, 56]), 56, a, i], WRITE_PACKET_SIZE)] };
    }

    public static Command CreateSetLightCommand(byte[] baseKeyboardConfig, Light light, int profileIndex = INDEX)
    {
        byte[] command = [.. baseKeyboardConfig];
        command[8] = (byte)light.Effect;
        command[9] = (byte)light.Brightness;
        command[10] = (byte)(4 - light.Speed).Clamp(0, 4);
        command[11] = (byte)light.Direction;
        command[12] = (byte)(light.SingleColor is 1 ? 0 : 1);
        var packets = ToPackets(CommandType.Lighting.As(), profileIndex * PROFILE_INDEX_SHIFT, command);
        return new() { Packets = packets };
    }

    public static IReadOnlyCollection<Command> BuildPackets(this Profile profileItem)
    {

        return [];
    }
}