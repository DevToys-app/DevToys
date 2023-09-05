using System.Threading;
using System.Threading.Tasks;
using DevToys.Core.Tools;

namespace DevToys.UnitTests.Core.Tools;

public class SmartDetectionServiceTests : MefBasedTest
{
    [Theory]
    [InlineData("hello world", true, "MockTool2")]
    [InlineData("hello world", false, "MockTool2")]
    [InlineData("{ \"json\": 123 }", true, "MockTool")]
    [InlineData("{ \"json\": 123 }", false, "MockTool", "MockTool2")]
    [InlineData(123, false)]
    public async Task DetectToolsAsync(object rawData, bool isStrict, params string[] toolNames)
    {
        SmartDetectionService smartDetectionService = MefProvider.Import<SmartDetectionService>();
        IReadOnlyList<SmartDetectedTool> detectedTools = await smartDetectionService.DetectAsync(rawData, isStrict, CancellationToken.None);

        detectedTools.Should().HaveSameCount(toolNames);
        for (int i = 0; i < toolNames.Length; i++)
        {
            detectedTools[i].ToolInstance.InternalComponentName.Should().Be(toolNames[i]);
        }
    }
}
