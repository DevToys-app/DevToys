using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions
{
    public interface IConverterContainer<T>
    {
        Dictionary<T, IConverterService> Services { get; }

        bool TryConvert(string input, out string output, T inputLang, T outputLang);
        void ConfigureService(T service, Action<IConverterService> configuration);
    }
}
