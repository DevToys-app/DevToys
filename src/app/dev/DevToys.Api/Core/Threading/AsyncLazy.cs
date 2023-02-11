﻿namespace DevToys.Api.Core.Threading;

[DebuggerDisplay($"IsValueCreated = {{{nameof(IsValueCreated)}}}")]
public class AsyncLazy<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
{
    private static Func<Task<T>> FromFuncT(Func<T> valueFunc)
    {
        Guard.IsNotNull(valueFunc);
        return () => Task.FromResult(valueFunc());
    }

    private readonly Lazy<Task<T>> _innerLazy;

    public bool IsValueCreated => _innerLazy.IsValueCreated && _innerLazy.Value.IsCompletedSuccessfully;

    public AsyncLazy(T value)
    {
        _innerLazy = new Lazy<Task<T>>(Task.FromResult(value));
    }

    public AsyncLazy(Func<T> valueFactory)
    {
        _innerLazy = new Lazy<Task<T>>(FromFuncT(valueFactory));
    }

    public AsyncLazy(Func<T> valueFactory, bool isThreadSafe)
    {
        _innerLazy = new Lazy<Task<T>>(FromFuncT(valueFactory), isThreadSafe);
    }

    public AsyncLazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
    {
        _innerLazy = new Lazy<Task<T>>(FromFuncT(valueFactory), mode);
    }

    public AsyncLazy(Func<Task<T>> valueFactory)
    {
        _innerLazy = new Lazy<Task<T>>(valueFactory);
    }

    public AsyncLazy(Func<Task<T>> valueFactory, bool isThreadSafe)
    {
        _innerLazy = new Lazy<Task<T>>(valueFactory, isThreadSafe);
    }

    public AsyncLazy(Func<Task<T>> valueFactory, LazyThreadSafetyMode mode)
    {
        _innerLazy = new Lazy<Task<T>>(valueFactory, mode);
    }

    public Task<T> GetValueAsync()
    {
        return IsValueCreated ? Task.FromResult(_innerLazy.Value.Result) : _innerLazy.Value;
    }
}
