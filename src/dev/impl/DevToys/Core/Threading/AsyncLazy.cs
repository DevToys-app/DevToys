#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevToys.Core.Threading
{
    internal sealed class AsyncLazy<T>
    {
        private static Func<Task<T>> FromFuncT(Func<T> valueFunc) => () => Task.FromResult(valueFunc());

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

        public async Task<T> GetValueAsync()
        {
            return IsValueCreated? await _innerLazy.Value : _innerLazy.Value.Result;
        }
    }
}
