using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.UnitTests.Tools.Helpers;

public class UuidHelperTests
{
    [Fact]
    internal void GenerateUuidVersionOne()
    {
        TestUuid(UuidVersion.One, hyphens: true, uppercase: true);
        TestUuid(UuidVersion.One, hyphens: false, uppercase: true);
        TestUuid(UuidVersion.One, hyphens: true, uppercase: false);
        TestUuid(UuidVersion.One, hyphens: false, uppercase: false);
    }

    [Fact]
    internal void GenerateUuidVersionFour()
    {
        TestUuid(UuidVersion.Four, hyphens: true, uppercase: true);
        TestUuid(UuidVersion.Four, hyphens: false, uppercase: true);
        TestUuid(UuidVersion.Four, hyphens: true, uppercase: false);
        TestUuid(UuidVersion.Four, hyphens: false, uppercase: false);
    }

    private void TestUuid(UuidVersion uuidVersion, bool hyphens, bool uppercase)
    {
        string newUuid
            = UuidHelper.GenerateUuid(
                uuidVersion,
                hyphens,
                uppercase);
        TestUuid(newUuid, hyphens, uppercase);
    }

    private void TestUuid(string uuid, bool hyphens, bool uppercase)
    {
        Guid.TryParse(uuid, out _).Should().BeTrue();
        uuid.Contains("-").Should().Be(hyphens);
        uuid.Length.Should().Be(hyphens ? 36 : 32);
        uuid.Should().Be(uppercase ? uuid.ToUpperInvariant() : uuid.ToLowerInvariant());
    }
}
