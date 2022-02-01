using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.ViewModels.Tools.Converters.JsonYaml.Services;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions
{
    public interface IConverterService
    {
        object Read(string input);
        string Write(object _object);

        void SetDeserializerConfigurations(ConverterConfiguration key, object value);
        void SetSerializerConfigurations(ConverterConfiguration key, object value);
    }
}
