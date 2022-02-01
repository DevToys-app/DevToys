using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.Converters.JsonYaml.Services.Exceptions
{
    internal class UnknownConfigurationException : Exception
    {
        public UnknownConfigurationException(string message) : base(message)
        {

        }
    }
}
