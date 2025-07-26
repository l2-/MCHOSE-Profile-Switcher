using AwesomeAssertions;
using Common;
using System.Text.Json;

namespace Driver.Test;

public class KeyboardProfile_specs
{
    [Test]
    public void Can_parse_keyboard_profile_file()
    {
        var text = File.ReadAllText("Data/cs2.json");
        var profile = text.Deserialize<KeyboardProfile>();
        profile.Should().NotBeNull();
    }
}
