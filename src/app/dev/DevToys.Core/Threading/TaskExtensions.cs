using System.Threading.Tasks;

namespace DevToys.Core.Threading;

/// <summary>
/// Provides a set of helper method to play around with threads.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Runs a task without waiting for its result.
    /// </summary>
    public static void Forget(this Task _)
    {
    }

    /// <summary>
    /// Runs a task without waiting for its result.
    /// </summary>
    public static void Forget<T>(this Task<T> _)
    {
    }

    /// <summary>
    /// Runs a task without waiting for its result. Swallows or handle any exception caused by the task.
    /// </summary>
    /// <param name="errorHandler">The action to run when an exception is caught.</param>
    public static async void ForgetSafely(this Task task, Action<Exception>? errorHandler = null)
    {
        try
        {
            await task.ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            errorHandler?.Invoke(ex);
        }
    }

    /// <summary>
    /// Gets the result of the task synchronously, on the current thread.
    /// </summary>
    public static void CompleteOnCurrentThread(this Task task)
    {
        task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the result of the task synchronously, on the current thread.
    /// </summary>
    public static T CompleteOnCurrentThread<T>(this Task<T> task)
    {
        return task.GetAwaiter().GetResult();
    }
}
