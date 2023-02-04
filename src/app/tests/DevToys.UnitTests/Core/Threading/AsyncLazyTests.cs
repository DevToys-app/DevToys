using System.Threading.Tasks;
using DevToys.Api.Core.Threading;

namespace DevToys.UnitTests.Core.Threading;

public class AsyncLazyTests
{
    [Fact]
    public async Task AccurateResultAsync()
    {
        var expected = new TestClass(5);
        var lazy = new AsyncLazy<TestClass>(async () =>
        {
            await Task.Yield();
            return expected;
        });

        TestClass actual = await lazy.GetValueAsync();
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task IsValueCreatedAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        var lazy = new AsyncLazy<TestClass>(async () =>
        {
            await tcs.Task;
            return new TestClass(5);
        });

        Assert.False(lazy.IsValueCreated);
        Task<TestClass> resultTask = lazy.GetValueAsync();
        Assert.False(lazy.IsValueCreated);
        tcs.SetResult(true);
        await resultTask;
        Assert.True(lazy.IsValueCreated);
        Assert.Equal(5, (await resultTask).Value);
    }

    [Fact]
    public void BadArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new AsyncLazy<object>(null));
    }

    public class TestClass
    {
        public int Value { get; }

        public TestClass(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is TestClass other)
            {
                return Value == other.Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}
