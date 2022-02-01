using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;
using DevToys.ViewModels.Tools.Converters.JsonYaml;
using DevToys.ViewModels.Tools.Converters.JsonYaml.Services;
using DevToys.ViewModels.Tools.Converters.JsonYaml.Services.Exceptions;
using DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Converters
{
    [ServiceType(GeneratorLanguages.Yaml)]
    class YamlConverterService : IConverterService
    {
        int Indent { get; set; } = 2;
        public object Read(string input)
        {
            using var stringReader = new StringReader(input);
            var deserializer = new DeserializerBuilder()
                .WithNodeTypeResolver(new DecimalYamlTypeResolver())
                .Build();
            return deserializer.Deserialize(stringReader);
        }

        public void SetDeserializerConfigurations(ConverterConfiguration key, object value)
        {
            // pass
        }

        public void SetSerializerConfigurations(ConverterConfiguration key, object value)
        {
            switch (key)
            {
                case ConverterConfiguration.Indentation:
                    ConfigureIndentation((Indentation)value);
                    break;
                default:
                    throw new UnknownConfigurationException("Unrecognized configuration parameter");
            }
        }

        public string Write(object input)
        {
            var serializer
                = Serializer.FromValueSerializer(
                    new SerializerBuilder().BuildValueSerializer(),
                    EmitterSettings.Default.WithBestIndent(Indent).WithIndentedSequences());

            return serializer.Serialize(input) ?? string.Empty;
        }

        private void ConfigureIndentation(Indentation indent)
        {
            Indent = indent switch
            {
                Indentation.TwoSpaces => 2,
                Indentation.FourSpaces => 4,
                _ => Indent
            };
        }
    }
}
