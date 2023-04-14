#nullable enable

using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DevToys.Helpers.JsonYaml.Core
{
    internal class BooleanYamlTypeResolver : INodeTypeResolver
    {
        public bool Resolve(NodeEvent? nodeEvent, ref Type currentType)
        {
            if (nodeEvent is Scalar scalar)
            {
                // avoid unnecessary parsing attempts
                bool couldBeBoolean
                    = scalar.Style is ScalarStyle.Plain
                    && (string.Equals(scalar.Value, bool.FalseString, StringComparison.OrdinalIgnoreCase) || string.Equals(scalar.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase));

                if (couldBeBoolean && bool.TryParse(scalar.Value, out _))
                {
                    currentType = typeof(bool);
                    return true;
                }
            }
            return false;
        }
    }
}
