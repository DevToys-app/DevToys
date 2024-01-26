using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.UnitTests.Tools.Helpers;

public class UuidHelperTests
{
    [Theory]
    [InlineData(UuidVersion.One, true, true)]
    [InlineData(UuidVersion.One, false, true)]
    [InlineData(UuidVersion.One, true, false)]
    [InlineData(UuidVersion.One, false, false)]
    [InlineData(UuidVersion.Four, true, true)]
    [InlineData(UuidVersion.Four, false, true)]
    [InlineData(UuidVersion.Four, true, false)]
    [InlineData(UuidVersion.Four, false, false)]
    internal void GenerateUuid(UuidVersion uuidVersion, bool hyphens, bool uppercase)
    {
        string newUuid
            = UuidHelper.GenerateUuid(
                uuidVersion,
                hyphens,
                uppercase);
        TestUuid(newUuid, hyphens, uppercase);
    }

    private static void TestUuid(string uuid, bool hyphens, bool uppercase)
    {
        Guid.TryParse(uuid, out _).Should().BeTrue();
        uuid.Contains('-').Should().Be(hyphens);
        uuid.Length.Should().Be(hyphens ? 36 : 32);
        uuid.Should().Be(uppercase ? uuid.ToUpperInvariant() : uuid.ToLowerInvariant());
    }
}
