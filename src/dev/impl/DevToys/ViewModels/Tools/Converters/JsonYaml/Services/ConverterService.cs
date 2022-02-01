#nullable enable
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;
using DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions;

namespace DevToys.ViewModels.Tools.JsonYaml.Services
{
    [Export(typeof(IConverterContainer<GeneratorLanguages>))]
    [Shared]
    public class ConverterService : IConverterContainer<GeneratorLanguages>
    {
        private readonly Lazy<Dictionary<GeneratorLanguages, IConverterService>> _lazyServices;
        public Dictionary<GeneratorLanguages, IConverterService> Services => _lazyServices.Value;

        public ConverterService()
        {
            _lazyServices = new Lazy<Dictionary<GeneratorLanguages, IConverterService>>(Initialize);
        }

        public bool TryConvert(string input, out string output, GeneratorLanguages inputLang, GeneratorLanguages outputLang)
        {
            if(!(Services.ContainsKey(inputLang) && Services.ContainsKey(outputLang)) ||
                string.IsNullOrWhiteSpace(input))
            {
                output = string.Empty;
                return false;
            }
            object? generic = Services[inputLang].Read(input);
            output = Services[outputLang].Write(generic);
            return true;
        }

        public Dictionary<GeneratorLanguages, IConverterService> Initialize()
        {
            var temp = new Dictionary<GeneratorLanguages, IConverterService>();
            foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes.Where(t => t.GetCustomAttribute<ServiceTypeAttribute>() is not null && t.GetInterface(nameof(IConverterService)) is not null))
            {
                var serviceAttribute = type.GetCustomAttribute<ServiceTypeAttribute>();
                if (Activator.CreateInstance(type) is not IConverterService service)
                {
                    // TODO: Should throw an exception or ignore type?
                    continue;
                }
                temp.Add(serviceAttribute.ServiceId, service);
            }
            return temp;
        }

        public void ConfigureService(GeneratorLanguages service, Action<IConverterService> configuration)
        {
            if(Services.TryGetValue(service, out IConverterService serv))
            {
                configuration(serv);
            }
        }
    }
}
