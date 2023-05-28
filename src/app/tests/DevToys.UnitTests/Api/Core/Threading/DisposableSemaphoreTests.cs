using System.Threading;
using System.Threading.Tasks;

namespace DevToys.UnitTests.Api.Core.Threading;

public class DisposableSemaphoreTests
{
    [Fact]
    public async Task DisposingTest()
    {
        var semaphore = new DisposableSemaphore(1);

        using (await semaphore.WaitAsync(CancellationToken.None))
        {
            semaphore.IsBusy.Should().BeTrue();
        }

        semaphore.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task ThreadingTest()
    {
        var semaphore = new DisposableSemaphore(1);
        int shared = 0;

        async Task TestFunc(int val)
        {
            using (await semaphore.WaitAsync(CancellationToken.None))
            {
                shared = val;
            }
        }

        var t1 = Task.Run(() => TestFunc(3));
        await t1;
        shared.Should().Be(3);

        var t2 = Task.Run(() => TestFunc(5));
        await t2;
        shared.Should().Be(5);

        var t3 = Task.Run(() => TestFunc(3));
        await t3;
        shared.Should().Be(3);
    }
}
