#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;

namespace DevToys.ViewModels.Tools.Converters.NumberBaseConverter
{
    internal class NumberBaseFormatBuilder
    {
        public string DisplayName { get; set; } = string.Empty;

        //public Radix Value { get; set; } = Radix.Custom;

        public int BaseNumber { get; set; }

        public int GroupSize { get; set; } = 4;

        public char GroupSeparator { get; set; } = ' ';

        public NumberBaseDictionary? Dictionary { get; set; } = null;

        private NumberBaseFormatBuilder() { }

        NumberBaseFormat Build()
        {
            if (BaseNumber < 2)
            {
                throw new ArgumentException("A base number must be specified and should be greater than 1");
            }
            if (Dictionary is not null && Dictionary.Dictionary.Length < BaseNumber)
            {
                throw new ArgumentException("The dictionary could not be smaller than the base number.");
            }
            if (NumberBaseDictionary.DefaultDictionary.Dictionary.Length < BaseNumber)
            {
                throw new ArgumentException("The dictionary could not be smaller than the base number.");
            }
            return new(DisplayName, Radix.Custom, BaseNumber, GroupSize, GroupSeparator, Dictionary);
        }

        internal static NumberBaseFormat BuildFormat(Action<NumberBaseFormatBuilder> configure)
        {
            var builder = new NumberBaseFormatBuilder();
            configure(builder);
            return builder.Build();
        }
    }
}

