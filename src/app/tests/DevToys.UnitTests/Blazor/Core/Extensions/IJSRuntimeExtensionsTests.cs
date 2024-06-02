using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using DevToys.Blazor.Core.Extensions;

namespace DevToys.UnitTests.Blazor.Core.Extensions;

public class IJSRuntimeExtensionsTests
{
    [Fact]
    public async Task InvokeVoidAsyncWithErrorHandling_NoException()
    {
        var runtimeMock = new Mock<IJSRuntime>(MockBehavior.Strict);

        runtimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>("myMethod", It.IsAny<object[]>()))
            .ReturnsAsync(Mock.Of<IJSVoidResult>()).Verifiable();

        IJSRuntime runtime = runtimeMock.Object;

        await runtime.InvokeVoidWithErrorHandlingAsync("myMethod", 42, "blub");

        runtimeMock.Verify();
    }

    public static IEnumerable<object[]> Exceptions
        = new List<object[]>
        {
#if DEBUG
#else
                new object[] { new JSException("only testing") },
#endif
                new object[] { new TaskCanceledException() },
                new object[] { new JSDisconnectedException("only testing") },
        };

    //[Theory]
    //[MemberData(nameof(Exceptions))]
    //public async Task InvokeVoidAsyncWithErrorHandling_Exception(Exception ex)
    //{
    //    var runtimeMock = new Mock<IJSRuntime>(MockBehavior.Strict);

    //    runtimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>("myMethod", It.IsAny<object[]>()))
    //        .Throws(ex).Verifiable();

    //    IJSRuntime runtime = runtimeMock.Object;

    //    await runtime.InvokeVoidWithErrorHandlingAsync("myMethod", 42, "blub");

    //    runtimeMock.Verify();
    //}
}
