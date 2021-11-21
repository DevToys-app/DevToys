using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevToys.Shared.Core.Threading
{
    /// <summary>
    /// Provides a set of helper method to play around with threads.
    /// </summary>
    internal static class TaskExtension
    {
        /// <summary>
        /// Runs a task without waiting for its result.
        /// </summary>
        internal static void Forget(this Task _)
        {
        }

        /// <summary>
        /// Runs a task without waiting for its result.
        /// </summary>
        internal static void Forget<T>(this Task<T> _)
        {
        }

        /// <summary>
        /// Runs a task without waiting for its result. Swallows or handle any exception caused by the task.
        /// </summary>
        /// <param name="errorHandler">The action to run when an exception is caught.</param>
        internal static async void ForgetSafely(this Task task, Action<Exception>? errorHandler = null)
        {
            try
            {
                await task.ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke(ex);
#if DEBUG
                Debugger.Launch();
#endif
            }
        }

        /// <summary>
        /// Gets an awaiter that schedules continuations on the specified scheduler.
        /// </summary>
        /// <param name="scheduler">The task scheduler used to execute continuations.</param>
        /// <returns>An awaitable.</returns>
        internal static TaskSchedulerAwaiter GetAwaiter(this TaskScheduler scheduler)
        {
            Arguments.NotNull(scheduler, nameof(scheduler));
            return new TaskSchedulerAwaiter(scheduler);
        }
    }
}
