using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.Converters.NumberBaseConverter.Exceptions
{
    internal class InvalidDictionaryBaseNumberPairException : Exception
    {
        public InvalidDictionaryBaseNumberPairException(string message) : base(message)
        {

        }
    }
}
