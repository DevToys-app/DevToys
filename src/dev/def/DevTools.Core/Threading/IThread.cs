using System;
using System.Threading.Tasks;

namespace DevTools.Core.Threading
{
    /// <summary>
    /// Provides a set of methods to play around with threads.
    /// </summary>
    public interface IThread
    {
        /// <summary>
        /// Throws an exception if the current thread isn't the UI thread.
        /// </summary>
        void ThrowIfNotOnUIThread();

        /// <summary>
        /// Throws an exception if the current thread is the UI thread.
        /// </summary>
        void ThrowIfOnUIThread();

        /// <summary>
        /// Runs a given action on the UI thread and wait for its result asynchronously.
        /// </summary>
        /// <param name="action">Action to run on the UI thread.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RunOnUIThreadAsync(Action action);

        /// <summary>
        /// Runs a given action on the UI thread and wait for its result asynchronously.
        /// </summary>
        /// <param name="action">Action to run on the UI thread.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RunOnUIThreadAsync(Func<Task> action);

        /// <summary>
        /// Runs a given action on the UI thread and wait for its result asynchronously.
        /// </summary>
        /// <param name="action">Action to run on the UI thread.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<T> RunOnUIThreadAsync<T>(Func<Task<T>> action);
    }
}
