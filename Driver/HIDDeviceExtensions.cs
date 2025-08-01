using HidSharp;
using System.Diagnostics;

namespace Driver;

public sealed record Base
{
    public required int ProfileIndex { get; init; }
}

public sealed record Info
{
    public required int FirmwareVersion { get; init; }
}

public static class HIDDeviceExtensions
{
    private const bool USE_HEX_FORMATTING = false;
    public static bool WRITE_PACKET_INFO_TO_CONSOLE { get; set; } = false;

    public static Info? GetInfo(this HidStream stream)
    {
        var response = stream.WriteCommand(Packets.InfoCommand);
        if (response.Count != 0 && response.First().Length != 0)
        {
            return response.First().Info();
        }
        return null;
    }

    public static Base? GetBase(this HidStream stream)
    {
        var response = stream.WriteCommand(Packets.BaseCommand);
        if (response.Count != 0 && response.First().Length != 0)
        {
            return response.First().Base();
        }
        return null;
    }

    public static KeyboardSpecs? GetKeyboardSpecs(this HidStream stream)
    {
        var info = stream.GetInfo();
        if (info is null) return null;
        var @base = stream.GetBase();
        if (@base is null) return null;
        return new KeyboardSpecs { Base = @base, Info = info };
    }

    public static void PushProfile(this HidStream stream, KeyboardProfile keyboardProfile, KeyboardProfile? lastProfile)
    {
        var timer = new Stopwatch();
        timer.Start();

        var @base = stream.GetKeyboardBasePacket();
        var commands = keyboardProfile.PushProfileCommands(@base);
        if (lastProfile is { })
        {
            // Small optimisation. Try to remove empty packets
            var lastCommands = lastProfile.PushProfileCommands(@base);
            var reducedCommands = commands.Reduce(lastCommands);
            commands = reducedCommands;
        }
        stream.WriteCommands(commands);
        Console.WriteLine("Pushed profile {0} - {1} packets in {2}ms", keyboardProfile.Detail.Name, commands.Sum(c => c.Packets.Count), timer.ElapsedMilliseconds);
    }

    public static byte[] GetKeyboardBasePacket(this HidStream stream)
    {
        var response = stream.WriteCommand(Packets.CreateGetKeyboardBasePacket());
        if (response.Count != 0 && response.First().Length != 0)
        {
            return response.First().BasePacket();
        }
        return [];
    }

    public static TResult Using<TResult, T>(
        this T factory,
        Func<T, TResult> use) where T : IDisposable
    {
        using var disposable = factory;
        return use(disposable);
    }

    public static string PacketToString(this byte[] packet)
        => string.Format("[{0}]", string.Join(" ", packet.Select(b => USE_HEX_FORMATTING ? string.Format("{0:x2}", b) : b.ToString())));

    public static byte[] WritePacket(this HidStream stream, byte[] packet)
    {
        if (packet.Length < 1) return [];
        if (packet.Length > 64)
        {
            throw new Exception(string.Format("Packet {0}, probably should be of length < 64", PacketToString(packet)));
        }
        if (packet.Length < 64)
        {
            Console.WriteLine(string.Format("Packet {0}, probably should be of length 64. {1}", PacketToString(packet), new System.Diagnostics.StackTrace()));
        }
        if (WRITE_PACKET_INFO_TO_CONSOLE)
        {
            Console.WriteLine("Writing packet \t{0}", packet.PacketToString());
        }
        try
        {
            stream.Write([0, .. packet]);
        }
        catch (Exception ex)
        {
            Console.WriteLine("FAILED TO WRITE PACKET {0}, {1}", packet, ex);
        }
        try
        {
            // Read after write is always required otherwise we risk the keyboard chatting on the same line at the same time.
            var response = stream.Read()[1..];
            if (WRITE_PACKET_INFO_TO_CONSOLE)
            {
                Console.WriteLine("Received packet {0}", response.ToArray().PacketToString());
            }
            return [.. response];
        }
        catch (Exception ex)
        {
            Console.WriteLine("FAILED TO READ PACKET {0}", ex);
        }
        return [];
    }

    public static IReadOnlyCollection<byte[]> WriteCommand(this HidStream stream, Command command)
        => [.. command.Packets.Select(p => WritePacket(stream, p))];

    public static IReadOnlyCollection<IReadOnlyCollection<byte[]>> WriteCommands(this HidStream stream, IReadOnlyCollection<Command> commands)
        => [.. commands.Select(command => command.Packets.Select(p => WritePacket(stream, p)).ToList())];
}

file static class ResponseExtensions
{
    private static int VersionFromBytes(byte b1, byte b2)
        => int.Parse(((b1 << 8) | b2).ToString("X"), System.Globalization.NumberStyles.Integer);

    public static Info? Info(this byte[] response)
    {
        if (response.Length < 12)
        {
            return null;
        }
        var firmware = VersionFromBytes(response[9], response[8]);
        return new Info { FirmwareVersion = firmware };
    }

    public static Base? Base(this byte[] response)
    {
        if (response.Length < 9)
        {
            return null;
        }
        return new Base { ProfileIndex = response[8] };
    }

    public static byte[] BasePacket(this byte[] response)
        => response[8..];
}