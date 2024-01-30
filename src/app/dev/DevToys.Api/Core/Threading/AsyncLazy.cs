namespace DevToys.Api;

/// <summary>
/// Represents an asynchronous lazy initialization.
/// </summary>
/// <typeparam name="T">The type of the value being lazily initialized.</typeparam>
[DebuggerDisplay($"IsValueCreated = {{{nameof(IsValueCreated)}}}")]
public class AsyncLazy<T>
{
    private readonly Lazy<Task<T>> _innerLazy;

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncLazy{T}"/> class with the specified value.
    /// </summary>
    /// <param name="value">The value to initialize the <see cref="AsyncLazy{T}"/> instance with.</param>
    public AsyncLazy(T value)
    {
        _innerLazy = new Lazy<Task<T>>(Task.FromResult(value));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncLazy{T}"/> class with the specified value factory.
    /// </summary>
    /// <param name="valueFactory">The delegate that represents the value factory.</param>
    public AsyncLazy(Func<T> valueFactory)
    {
        _innerLazy = new Lazy<Task<T>>(FromFuncT(valueFactory));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncLazy{T}"/> class with the specified value factory and thread safety option.
    /// </summary>
    /// <param name="valueFactory">The delegate that represents the value factory.</param>
    /// <param name="isThreadSafe">A boolean value indicating whether the lazy initialization is thread-safe.</param>
    public AsyncLazy(Func<T> valueFactory, bool isThreadSafe)
    {
        _innerLazy = new Lazy<Task<T>>(FromFuncT(valueFactory), isThreadSafe);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncLazy{T}"/> class with the specified value factory and thread safety mode.
    /// </summary>
    /// <param name="valueFactory">The delegate that represents the value factory.</param>
    /// <param name="mode">The thread safety mode for the lazy initialization.</param>
    public AsyncLazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
    {
        _innerLazy = new Lazy<Task<T>>(FromFuncT(valueFactory), mode);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncLazy{T}"/> class with the specified asynchronous value factory.
    /// </summary>
    /// <param name="valueFactory">The delegate that represents the asynchronous value factory.</param>
    public AsyncLazy(Func<Task<T>> valueFactory)
    {
        _innerLazy = new Lazy<Task<T>>(valueFactory);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncLazy{T}"/> class with the specified asynchronous value factory and thread safety option.
    /// </summary>
    /// <param name="valueFactory">The delegate that represents the asynchronous value factory.</param>
    /// <param name="isThreadSafe">A boolean value indicating whether the lazy initialization is thread-safe.</param>
    public AsyncLazy(Func<Task<T>> valueFactory, bool isThreadSafe)
    {
        _innerLazy = new Lazy<Task<T>>(valueFactory, isThreadSafe);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncLazy{T}"/> class with the specified asynchronous value factory and thread safety mode.
    /// </summary>
    /// <param name="valueFactory">The delegate that represents the asynchronous value factory.</param>
    /// <param name="mode">The thread safety mode for the lazy initialization.</param>
    public AsyncLazy(Func<Task<T>> valueFactory, LazyThreadSafetyMode mode)
    {
        _innerLazy = new Lazy<Task<T>>(valueFactory, mode);
    }

    /// <summary>
    /// Gets a task that represents the asynchronous initialization of the value.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization of the value.</returns>
    public Task<T> GetValueAsync()
    {
        return IsValueCreated ? Task.FromResult(_innerLazy.Value.Result) : _innerLazy.Value;
    }

    /// <summary>
    /// Determines whether the value has been created.
    /// </summary>
    public bool IsValueCreated => _innerLazy.IsValueCreated && _innerLazy.Value.IsCompletedSuccessfully;

    private static Func<Task<T>> FromFuncT(Func<T> valueFunc)
    {
        Guard.IsNotNull(valueFunc);
        return () => Task.FromResult(valueFunc());
    }
}
