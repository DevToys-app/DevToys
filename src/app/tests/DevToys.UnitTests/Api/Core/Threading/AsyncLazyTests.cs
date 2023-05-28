using System.Threading.Tasks;

namespace DevToys.UnitTests.Api.Core.Threading;

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
        actual.Should().BeSameAs(expected);
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

        lazy.IsValueCreated.Should().BeFalse();
        Task<TestClass> resultTask = lazy.GetValueAsync();
        lazy.IsValueCreated.Should().BeFalse();
        tcs.SetResult(true);
        await resultTask;
        lazy.IsValueCreated.Should().BeTrue();
        (await resultTask).Value.Should().Be(5);
    }

    [Fact]
    public void BadArguments()
    {
        Action act = () => new AsyncLazy<object>(null);
        act.Should().Throw<ArgumentNullException>();
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
                return Value == other.Value;

            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}
