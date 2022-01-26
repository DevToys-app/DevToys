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

        internal string? Get(Token token)
        {
            if (_params is null)
            {
                return token.Value;
            }

            if (!string.IsNullOrEmpty(token.Key))
            {
                return _params[token.Key!];
            }

            return _params.Values.ToArray()[_index++];
        }
    }
}
