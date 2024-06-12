using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Core;
using DevToys.Core.Models;
using DevToys.Core.Version;
using DevToys.Core.Web;

namespace DevToys.UnitTests.Core;

public class AppHelperTests
{
    [Theory]
    [InlineData(new[] { "" }, "tool", "")]
    [InlineData(new[] { "tool" }, "tool", "")]
    [InlineData(new[] { "--tool:" }, "tool", "")]
    [InlineData(new[] { "--tool:value" }, "tool", "value")]
    [InlineData(new[] { "--tool:", "value" }, "tool", "value")]
    [InlineData(new[] { "--tool:", "\"value with space\"" }, "tool", "value with space")]
    public void GetCommandLineArgument(string[] arguments, string searchedArgumentName, string expectedResult)
    {
        AppHelper.GetCommandLineArgument(arguments, searchedArgumentName).Should().Be(expectedResult);
    }

    [Fact]
    public async Task CheckForUpdate_PreviewAsync()
    {
        GitHubRelease[] releases
            = [
                new GitHubRelease
                {
                    Draft = true,
                    PreRelease = true,
                    Name = "v10.9.0"
                },
                new GitHubRelease
                {
                    Draft = false,
                    PreRelease = false,
                    Name = "v10.8.0.0"
                },
                new GitHubRelease
                {
                    Draft = false,
                    PreRelease = true,
                    Name = "v10.9.2"
                },
                new GitHubRelease
                {
                    Draft = false,
                    PreRelease = true,
                    Name = "v10.9.1"
                }
              ];
        string releasesJson = JsonSerializer.Serialize(releases);

        var mockWebClientService = new Mock<IWebClientService>();
        mockWebClientService
            .Setup(service => service.SafeGetStringAsync(It.IsAny<Uri>(), CancellationToken.None))
            .ReturnsAsync(releasesJson);

        var mockVersionService = new Mock<IVersionService>();
        mockVersionService
            .Setup(service => service.IsPreviewVersion())
            .Returns(true);

        bool result = await AppHelper.CheckForUpdateAsync(mockWebClientService.Object, mockVersionService.Object, CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckForUpdate_Preview_NoUpdate_Async()
    {
        GitHubRelease[] releases
            = [
                new GitHubRelease
                {
                    Draft = true,
                    PreRelease = true,
                    Name = "v10.9.3"
                },
                new GitHubRelease
                {
                    Draft = false,
                    PreRelease = false,
                    Name = "v10.8.0.0"
                }
              ];
        string releasesJson = JsonSerializer.Serialize(releases);

        var mockWebClientService = new Mock<IWebClientService>();
        mockWebClientService
            .Setup(service => service.SafeGetStringAsync(It.IsAny<Uri>(), CancellationToken.None))
            .ReturnsAsync(releasesJson);

        var mockVersionService = new Mock<IVersionService>();
        mockVersionService
            .Setup(service => service.IsPreviewVersion())
            .Returns(true);

        bool result = await AppHelper.CheckForUpdateAsync(mockWebClientService.Object, mockVersionService.Object, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForUpdate_Preview_NoUpdate2_Async()
    {
        GitHubRelease[] releases
            = [
                new GitHubRelease
                {
                    Draft = false,
                    PreRelease = true,
                    Name = "v0.0.0"
                },
              ];
        string releasesJson = JsonSerializer.Serialize(releases);

        var mockWebClientService = new Mock<IWebClientService>();
        mockWebClientService
            .Setup(service => service.SafeGetStringAsync(It.IsAny<Uri>(), CancellationToken.None))
            .ReturnsAsync(releasesJson);

        var mockVersionService = new Mock<IVersionService>();
        mockVersionService
            .Setup(service => service.IsPreviewVersion())
            .Returns(true);

        bool result = await AppHelper.CheckForUpdateAsync(mockWebClientService.Object, mockVersionService.Object, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForUpdate_StableAsync()
    {
        GitHubRelease[] releases
            = [
                new GitHubRelease
                {
                    Draft = false,
                    PreRelease = false,
                    Name = "v10.8.0.0"
                }
              ];
        string releasesJson = JsonSerializer.Serialize(releases);

        var mockWebClientService = new Mock<IWebClientService>();
        mockWebClientService
            .Setup(service => service.SafeGetStringAsync(It.IsAny<Uri>(), CancellationToken.None))
            .ReturnsAsync(releasesJson);

        var mockVersionService = new Mock<IVersionService>();
        mockVersionService
            .Setup(service => service.IsPreviewVersion())
            .Returns(false);

        bool result = await AppHelper.CheckForUpdateAsync(mockWebClientService.Object, mockVersionService.Object, CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckForUpdate_Stable_NoUpdate_Async()
    {
        GitHubRelease[] releases
            = [
                new GitHubRelease
                {
                    Draft = true,
                    PreRelease = false,
                    Name = "v10.9.0.3"
                },
                new GitHubRelease
                {
                    Draft = false,
                    PreRelease = true,
                    Name = "v10.8.0"
                }
              ];
        string releasesJson = JsonSerializer.Serialize(releases);

        var mockWebClientService = new Mock<IWebClientService>();
        mockWebClientService
            .Setup(service => service.SafeGetStringAsync(It.IsAny<Uri>(), CancellationToken.None))
            .ReturnsAsync(releasesJson);

        var mockVersionService = new Mock<IVersionService>();
        mockVersionService
            .Setup(service => service.IsPreviewVersion())
            .Returns(false);

        bool result = await AppHelper.CheckForUpdateAsync(mockWebClientService.Object, mockVersionService.Object, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForUpdate_Failed_Async()
    {
        string releasesJson = "foo";

        var mockWebClientService = new Mock<IWebClientService>();
        mockWebClientService
            .Setup(service => service.SafeGetStringAsync(It.IsAny<Uri>(), CancellationToken.None))
            .ReturnsAsync(releasesJson);

        var mockVersionService = new Mock<IVersionService>();
        mockVersionService
            .Setup(service => service.IsPreviewVersion())
            .Returns(false);

        bool result = await AppHelper.CheckForUpdateAsync(mockWebClientService.Object, mockVersionService.Object, CancellationToken.None);
        result.Should().BeFalse();
    }
}
