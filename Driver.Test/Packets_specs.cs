using AwesomeAssertions;
using Common;

namespace Driver.Test;

public partial class Packets_specs
{
    private KeyboardProfile cs2Profile;
    private readonly byte[] baseKeyboardConfig = [80, 0, 170, 187, 1, 1, 0, 12, 19, 100, 4, 1, 1, 0, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 2, 0, 0, 255, 0, 0, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16];

    [SetUp]
    public void Setup()
    {
        var text = File.ReadAllText("Data/cs2.json");
        cs2Profile = text.Deserialize<KeyboardProfile>() ?? throw new Exception("Profile cant be null");
    }


    [Test]
    public void Info_packet_correctly_formed()
    {
        Packets.InfoCommand
            .Should()
            .BeEquivalentTo(new Command()
            {
                Packets =
                [[85, 3, 0, 32, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]]
            });
    }

    [Test]
    public void Prefix_formed_correctly()
    {
        var prefix = Packets.Prefix(CommandType.Lighting.As(), 0, 0, baseKeyboardConfig);
        prefix.Should().BeEquivalentTo([85, 6, 0, 92, 56, 0, 0, 0]);
    }

    [Test]
    public void Get_packet_correctly_formed()
    {
        Packets.CreateGetKeyboardBasePacket()
            .Should()
            .BeEquivalentTo(new Command()
            {
                Packets =
                [[85, 5, 0, 56, 56, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]]
            });
    }

    [Test]
    public void SetLight_packet_correctly_formed()
    {
        cs2Profile.CreateSetLightCommand(baseKeyboardConfig)
            .Should()
            .BeEquivalentTo(new Command()
            {
                Packets =
                [[85, 6, 0, 92, 56, 0, 0, 0, 80, 0, 170, 187, 1, 1, 0, 12, 19, 100, 4, 1, 1, 0, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 2, 0, 0, 255, 0, 0, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16]]
            });

    }
}
