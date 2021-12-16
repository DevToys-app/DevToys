using System;
using System.Threading.Tasks;
using DevToys.Core.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Core.Threading
{
    [TestClass]
    public class AsyncLazyTests
    {
        [TestMethod]
        public async Task AccurateResultAsync()
        {
            var expected = new TestClass(5);
            var lazy = new AsyncLazy<TestClass>(async () =>
            {
                await Task.Yield();
                return expected;
            });

            TestClass actual = await lazy.GetValueAsync();
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public async Task IsValueCreatedAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var lazy = new AsyncLazy<TestClass>(async () =>
            {
                await tcs.Task;
                return new TestClass(5);
            });

            Assert.IsFalse(lazy.IsValueCreated);
            Task<TestClass> resultTask = lazy.GetValueAsync();
            Assert.IsFalse(lazy.IsValueCreated);
            tcs.SetResult(true);
            await resultTask;
            Assert.IsTrue(lazy.IsValueCreated);
            Assert.AreEqual(5, (await resultTask).Value);
        }

        [TestMethod]
        public void BadArguments()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AsyncLazy<object>(null));
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
}
