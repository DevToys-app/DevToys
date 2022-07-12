#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace DevToys.Helpers.SqlFormatter.Core
{
    /// <summary>
    /// Handles placeholder replacement with given params.
    /// </summary>
    internal sealed class Params
    {
        private readonly IReadOnlyDictionary<string, string>? _params;
        private int _index;

        public Params(IReadOnlyDictionary<string, string>? parameters)
        {
            _params = parameters;
        }

        internal string? Get(string key)
        {
            if (_params is null)
            {
                return null;
            }

            if (key is not null && key.Length != 0)
            {
                _params.TryGetValue(key, out string? paramValue);
                return paramValue;
            }

            return _params.ElementAtOrDefault(_index++).Value ?? null;
        }
    }
}
