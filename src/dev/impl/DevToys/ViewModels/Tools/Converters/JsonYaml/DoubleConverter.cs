#nullable enable

using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DevToys.ViewModels.Tools.JsonYaml
{
    internal class DoubleConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(double);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            string formatted = ((double?)value)?.ToString("r", System.Globalization.CultureInfo.InvariantCulture) ?? "";
            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, formatted, ScalarStyle.Any, true, false));
        }
    }
}
