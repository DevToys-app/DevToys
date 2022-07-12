using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.Converters.NumberBaseConverter.Exceptions
{
    internal class InvalidDictionarySizeException : Exception
    {
        public InvalidDictionarySizeException(string message) : base(message)
        {

        }
    }
}
