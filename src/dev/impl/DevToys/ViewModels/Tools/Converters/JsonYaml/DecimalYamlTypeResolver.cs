#nullable enable
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DevToys.ViewModels.Tools.Converters.JsonYaml
{
    internal class DecimalYamlTypeResolver : INodeTypeResolver
    {
        public bool Resolve(NodeEvent? nodeEvent, ref Type currentType)
        {
            if (nodeEvent is Scalar scalar)
            {
                // avoid unnecessary parsing attempts
                bool couldBeNumber =
                    (scalar.Style is not ScalarStyle.SingleQuoted and not ScalarStyle.DoubleQuoted) &&
                    scalar.Value.Length != 0 &&
                    (scalar.Value[0] is >= '0' and <= '9' || scalar.Value[0] == '-');

                if (couldBeNumber && decimal.TryParse(scalar.Value, out _))
                {
                    currentType = typeof(decimal);
                    return true;
                }
            }
            return false;
        }
    }
}
