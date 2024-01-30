namespace DevToys.Api;

/// <summary>
/// Provides a set of helper method to play around with threads.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Runs a task without waiting for its result.
    /// </summary>
    /// <param name="_">The task to run.</param>
    public static void Forget(this Task _)
    {
    }

    /// <summary>
    /// Runs a task without waiting for its result.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="_">The task to run.</param>
    public static void Forget<T>(this Task<T> _)
    {
    }

    /// <summary>
    /// Runs a task without waiting for its result.
    /// </summary>
    /// <param name="_">The task to run.</param>
    public static void Forget(this ValueTask _)
    {
    }

    /// <summary>
    /// Runs a task without waiting for its result.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="_">The task to run.</param>
    public static void Forget<T>(this ValueTask<T> _)
    {
    }

    /// <summary>
    /// Runs a task without waiting for its result. Swallows or handle any exception caused by the task.
    /// </summary>
    /// <param name="task">The task to run.</param>
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
    /// Runs a task without waiting for its result. Swallows or handle any exception caused by the task.
    /// </summary>
    /// <param name="task">The task to run.</param>
    /// <param name="errorHandler">The action to run when an exception is caught.</param>
    public static async void ForgetSafely(this ValueTask task, Action<Exception>? errorHandler = null)
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
    /// <param name="task">The task to complete.</param>
    public static void CompleteOnCurrentThread(this Task task)
    {
        task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the result of the task synchronously, on the current thread.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="task">The task to complete.</param>
    /// <returns>The result of the task.</returns>
    public static T CompleteOnCurrentThread<T>(this Task<T> task)
    {
        return task.GetAwaiter().GetResult();
    }
}
