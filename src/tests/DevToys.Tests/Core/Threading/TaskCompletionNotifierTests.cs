using System;
using System.Threading.Tasks;
using DevToys.Core.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Core.Threading
{
    [TestClass]
    public class TaskCompletionNotifierTests
    {
        [TestMethod]
        public async Task TaskCompletionNotifierFaultedAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var tcn 
                = new TaskCompletionNotifier<int>(
                    async () =>
                    {
                        await tcs.Task;
                        throw new Exception("Task faulted!");
                    });

            Assert.IsFalse(tcn.IsFaulted);
            Assert.IsFalse(tcn.IsCanceled);
            Assert.IsFalse(tcn.IsSuccessfullyCompleted);
            Assert.IsFalse(tcn.IsCompleted);

            tcs.TrySetResult(true);
            await Task.Delay(100);

            Assert.IsTrue(tcn.IsFaulted);
            Assert.IsFalse(tcn.IsCanceled);
            Assert.IsFalse(tcn.IsSuccessfullyCompleted);
            Assert.IsTrue(tcn.IsCompleted);
        }

        [TestMethod]
        public async Task TaskCompletionNotifierCanceledAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var tcn
                = new TaskCompletionNotifier<int>(
                    async () =>
                    {
                        await tcs.Task;
                        throw new OperationCanceledException("Task canceled!");
                    });

            Assert.IsFalse(tcn.IsFaulted);
            Assert.IsFalse(tcn.IsCanceled);
            Assert.IsFalse(tcn.IsSuccessfullyCompleted);
            Assert.IsFalse(tcn.IsCompleted);

            tcs.TrySetResult(true);
            await Task.Delay(100);

            Assert.IsFalse(tcn.IsFaulted);
            Assert.IsTrue(tcn.IsCanceled);
            Assert.IsFalse(tcn.IsSuccessfullyCompleted);
            Assert.IsTrue(tcn.IsCompleted);
        }

        [TestMethod]
        public async Task TaskCompletionNotifierSuccessfulAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var tcn
                = new TaskCompletionNotifier<int>(
                    async () =>
                    {
                        await tcs.Task;
                        return 5;
                    });

            Assert.IsFalse(tcn.IsFaulted);
            Assert.IsFalse(tcn.IsCanceled);
            Assert.IsFalse(tcn.IsSuccessfullyCompleted);
            Assert.IsFalse(tcn.IsCompleted);

            tcs.TrySetResult(true);
            await Task.Delay(100);

            Assert.IsFalse(tcn.IsFaulted);
            Assert.IsFalse(tcn.IsCanceled);
            Assert.IsTrue(tcn.IsSuccessfullyCompleted);
            Assert.IsTrue(tcn.IsCompleted);
            Assert.AreEqual(5, tcn.Result);
        }

        [TestMethod]
        public void BadArguments()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TaskCompletionNotifier<int>(null));
        }
    }
}
