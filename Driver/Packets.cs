using Common;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

namespace Driver;

public sealed record Command
{
    public required IReadOnlyCollection<byte[]> Packets { get; set; }
}

public static class Packets
{
    // Seems to always be 0 on my keyboard. Likely because it doesn't support storing multiple profiles onboard.
    const int INDEX = 0;
    const int WRITE_PACKET_SIZE = 64;
    const int PAYLOAD_SIZE = 56;
    const int PROFILE_INDEX_SHIFT = 64;
    const byte PACKET_CONST = 85;
    public static readonly Command InfoCommand = new() { Packets = [Raw(PACKET_CONST, CommandType.Info.As(), 0, 32, 32)] };
    public static readonly Command BaseCommand = new() { Packets = [Raw(PACKET_CONST, CommandType.Base.As(), 0, 32, 32)] };

    public static byte Sum(this IEnumerable<byte> command)
        => (byte)(command.Sum(s => s) & 255);

    public static (byte s, byte l) SplitToTwoBytes(int e)
        => ((byte)(255 & e), (byte)(e >> 8));

    public static byte[] Prefix(byte commandType, int sliceIndex, int shiftedProfileIndex, byte[] command)
    {
        var length = (byte)command.Length;
        var (s, l) = SplitToTwoBytes(sliceIndex + shiftedProfileIndex);
        var sum = command.Concat([length, s, l]).Sum();
        return [PACKET_CONST, commandType, 0, sum, length, s, l, 0];
    }

    public static byte[] RightPad(byte[] arr, int totalSize, byte value = 0)
        => [.. arr, .. Enumerable.Repeat(value, totalSize - arr.Length)];

    public static byte[] Raw(byte type, params byte[] command)
        => RightPad([type, .. command], WRITE_PACKET_SIZE);

    public static IReadOnlyCollection<byte[]> ToPackets(byte commandType, int shiftedProfileIndex, byte[] command, byte padValue = 0)
    {
        var packets = new List<byte[]>();
        for (int i = 0; i < command.Length;)
        {
            var sliceLength = Math.Min(command.Length - i, PAYLOAD_SIZE);
            var slice = command[i..(i + sliceLength)];
            var prefix = Prefix(commandType, i, shiftedProfileIndex, slice);
            byte[] packet = [.. prefix, .. RightPad(slice, PAYLOAD_SIZE, padValue)];
            packets.Add(packet);
            i += sliceLength;
        }
        return packets;
    }

    // getFuncConfig
    public static Command CreateGetKeyboardBasePacket(int profileIndex = INDEX)
    {
        var (a, i) = SplitToTwoBytes(PROFILE_INDEX_SHIFT * profileIndex);
        return new() { Packets = [RightPad([PACKET_CONST, CommandType.GetBasePacket.As(), 0, Sum([a, i, 56]), 56, a, i], WRITE_PACKET_SIZE)] };
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

    public static Command CreateSetAllUserKeys(UserKeys userKeys, int layer, int profileIndex = INDEX)
    {
        IReadOnlyCollection<UserKey> keys = layer switch
        {
            0 => userKeys.Layer1,
            1 => userKeys.Layer2,
            2 => userKeys.Layer3,
            3 => userKeys.Layer4,
            _ => throw new NotImplementedException("Layer doesn't exist?"),
        };
        byte[] command = [.. keys.SelectMany(key => new byte[] { (byte)key.Type, (byte)key.Code1, (byte)key.Code2 })];
        var packets = ToPackets(CommandType.SetAllUserKeys.As(), 512 * layer + 3 * 0 + 2048 * profileIndex, command);
        return new() { Packets = packets };
    }

    private static byte[] CreateTravelKeyCommand(TravelKey travelKey)
    {
        var (actuationS, actuationL) = SplitToTwoBytes(Math.Max(0, travelKey.KeyActuation - 1));
        actuationL = (byte)(actuationL & 1);
        actuationL |= (byte)(travelKey.PressPrecision & 3);
        actuationL |= (byte)(travelKey.ReleasePrecision & 3);
        var (rtPressS, rtPressL) = SplitToTwoBytes(Math.Max(0, travelKey.RtPress - 1));
        rtPressL |= (byte)(Math.Max(0, travelKey.PressDeadzone) << 1);
        var (rtReleaseS, rtReleaseL) = SplitToTwoBytes(Math.Max(0, travelKey.RtRelease - 1));
        rtReleaseL |= (byte)(Math.Max(0, travelKey.ReleaseDeadzone) << 1);
        return [
                (byte)(travelKey.SwitchType | 160),
                (byte)(travelKey.Priority > 0 ? ((travelKey.Priority << 4) | travelKey.Keymode) : travelKey.Keymode),
                actuationS,
                actuationL,
                rtPressS,
                rtPressL,
                rtReleaseS,
                rtReleaseL,
            ];
    }

    public static Command CreateSetAllTravelKeys(IReadOnlyCollection<TravelKey> travelKeys, int profileIndex = INDEX)
    {
        byte[] command = [.. travelKeys.SelectMany(CreateTravelKeyCommand)];
        var packets = ToPackets(CommandType.SetTravelKeys.As(), 1024 * profileIndex, command);
        return new() { Packets = packets };
    }

    private static List<(int Code, string Action, int Delay)> CreateMacroCommand(Macro macro, int index)
    {
        var o = new List<(int, string, int)>();
        foreach (var item in macro.MacroActions)
        {
            if (item.Type.Equals("action", StringComparison.CurrentCultureIgnoreCase))
            {
                var code = item.WebCode switch
                {
                    "0" => 1,
                    "1" => 4,
                    "2" => 2,
                    "3" => 8,
                    "4" => 16,
                    _ => item.Code ?? 0,
                };
                o.Add((code, item.Action ?? string.Empty, item.Delay ?? 0));
            }
            else if (item.Type.Equals("time", StringComparison.CurrentCultureIgnoreCase))
            {
                o[^1] = (o[^1].Item1, o[^1].Item2, o[^1].Item3 + (item.Delay ?? 0));
            }
        }
        return o;
    }

    // Seriously what the fuck is this packet layout MCHOSE
    public static Command CreateSetMacro(IReadOnlyCollection<Macro> macros, int profileIndex = INDEX)
    {
        var lists = macros.Select(CreateMacroCommand)
            .Zip(macros)
            .Select((pair) => (pair.Second.Type, pair.First))
            .ToArray<(int Type, List<(int Code, string Action, int Delay)> List)>();
        var listCountsAggregate = new int[lists.Length];
        for (int i = 0; i < listCountsAggregate.Length - 1; i++)
        {
            listCountsAggregate[i + 1] = lists[i].List.Count * 4 + listCountsAggregate[i];
        }
        var a = RightPad([.. lists.SelectMany((item, i) =>
        {
            if (item.List.Count == 0)
            {
                return [64, 0];
            }
            var (s, l) = SplitToTwoBytes(listCountsAggregate[i] + 68);
            return new byte[] { s, l };
        })], 64);
        var o = lists.SelectMany(listItem => listItem.List.SelectMany((item, i) =>
        {
            var (delayS, delayL) = SplitToTwoBytes(item.Delay == 0 ? 2 : item.Delay);
            var n = i == listItem.List.Count - 1 ? 1 : 0;
            var l = item.Action.ToLower() is "keydown" or "mousedown" ? 1 : 0;
            var c = (byte)((item.Code >= 224 ? 1 : item.Action.ToLower() is "mousedown" or "mouseup" ? 3 : 2)
                | (l << 6)
                | (n << 7));
            var d = (byte)(item.Code >= 224 ? 1 << (15 & item.Code) : item.Code);
            return new byte[] { delayS, delayL, c, d };
        })).ToArray();
        if (o.Length != listCountsAggregate[^1])
        {
            throw new Exception($"Index {listCountsAggregate[^1]} not matching size of o {o.Length}");
        }
        byte[] command = [.. a, 0, 0, 128, 0, .. o];
        var packets = ToPackets(CommandType.SetMacro.As(), 2048 * profileIndex, command);
        return new() { Packets = packets, };
    }

    public static Command CreateSetAllTglKeyInfo(IReadOnlyCollection<Tglkey> tglKeys, int profileIndex = INDEX)
    {
        var command = RightPad([.. tglKeys.SelectMany(tglKey => new byte[] { (byte)tglKey.Type, (byte)tglKey.Code1, (byte)tglKey.Code2 })], 128);
        var packets = ToPackets(CommandType.SetTglKeyInfo.As(), 128 * profileIndex, command);
        return new() { Packets = packets };
    }

    public static Command CreateSetAllMtKeyInfo(IReadOnlyCollection<(AdvancedKeySimpleSettings MtClickKey, AdvancedKeySimpleSettings MtDownKey)> mtKeys, int profileIndex = INDEX)
    {
        var command = RightPad([.. mtKeys.SelectMany(mtKey => new byte[] { (byte)mtKey.MtClickKey.Type, (byte)mtKey.MtClickKey.Code1, (byte)mtKey.MtClickKey.Code2, (byte)mtKey.MtDownKey.Type, (byte)mtKey.MtDownKey.Code1, (byte)mtKey.MtDownKey.Code2 })], 256);
        var packets = ToPackets(CommandType.SetMtKeyInfo.As(), 256 * profileIndex, command);
        return new() { Packets = packets };
    }

    private static byte DksStatusToBit(int state)
        => (byte)((state > 0 ? 1 : 0) | ((state > 1 ? 1 : 0) << 1) | ((state > 2 ? 1 : 0) << 2));

    private static byte[] PacketForDksKeySetting(IReadOnlyCollection<int>? DksPoints, IReadOnlyCollection<DksKey> DksKeys)
    {
        var dksPoint1 = (byte)(DksPoints?.First() ?? 10);
        var dksPoint2 = (byte)(DksPoints?.Skip(1).First() ?? 30);
        var dksPoint3 = (byte)(DksPoints?.Skip(2).First() ?? 30);
        var dksPoint4 = (byte)(DksPoints?.Skip(3).First() ?? 10);
        byte[] packet = [dksPoint1, dksPoint2, dksPoint3, dksPoint4,
            .. DksKeys.SelectMany(dksKey =>
            {
                var downStart = dksKey.DownStart;
                var downEnd = dksKey.DownEnd;
                var upStart = dksKey.UpStart;
                var upEnd = dksKey.UpEnd;
                if (dksKey.DownStart == 4)
                {
                    downStart = 3;
                    downEnd = 3;
                    upStart = 2;
                }
                if (dksKey.DownStart == 3)
                {
                    downStart = 3;
                    downEnd = 2;
                }
                if (dksKey.DownEnd == 3)
                {
                    upStart = 2;
                }
                var downStartBit = DksStatusToBit(downStart);
                var downEndBit = DksStatusToBit(downEnd);
                var upStartBit = DksStatusToBit(upStart);
                var upEndBit = DksStatusToBit(upEnd);
                var full = (7 & downStartBit) | ((downEndBit << 3) & 56) | ((upStartBit << 6) & 448) | ((upEndBit << 9) & 512);
                var (s, l) = SplitToTwoBytes(full);
                return new byte[] { (byte)dksKey.Key.Type, (byte)dksKey.Key.Code1, (byte)dksKey.Key.Code2, s, l };
            })];
        return RightPad(packet, 24);
    }

    public static Command CreateSetAllDksKeyInfo(
        IReadOnlyCollection<(IReadOnlyCollection<int>? DksPoints, IReadOnlyCollection<DksKey> DksKeys)> advancedKeys,
        int profileIndex = INDEX)
    {
        var command = RightPad([.. advancedKeys
            .SelectMany(key => PacketForDksKeySetting(key.DksPoints, key.DksKeys))], 768);
        var packets = ToPackets(CommandType.SetDksKeyInfo.As(), 768 * profileIndex, command);
        return new() { Packets = packets };
    }
}