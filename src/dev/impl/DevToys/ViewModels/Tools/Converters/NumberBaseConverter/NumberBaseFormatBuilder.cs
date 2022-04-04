#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;
using DevToys.ViewModels.Tools.Converters.NumberBaseConverter.Exceptions;

namespace DevToys.ViewModels.Tools.Converters.NumberBaseConverter
{
    internal class NumberBaseFormatBuilder
    {
        public string DisplayName { get; set; } = string.Empty;

        public int BaseNumber { get; set; }

        public int GroupSize { get; set; } = 4;

        public char GroupSeparator { get; set; } = ' ';

        public NumberBaseDictionary? Dictionary { get; set; } = null;

        Func<NumberBaseFormat> Build { get; }

        private NumberBaseFormatBuilder()
        {
            Build = BuildNew;
        }
        private NumberBaseFormatBuilder(NumberBaseFormat _format)
        {
            Build = GetModifier(_format);
        }

        void AssertBuilder()
        {
            if (Dictionary is not null)
            {
                if (Dictionary.Dictionary.Length < 2)
                {
                    throw new InvalidDictionarySizeException("Dictionary size should be greater than 1.");
                }
                if (Dictionary.Dictionary.Length < BaseNumber)
                {
                    throw new InvalidDictionaryBaseNumberPairException("Dictionary size could not be smaller than the base number.");
                }
            }
            else if(NumberBaseDictionary.DefaultDictionary.Dictionary.Length < BaseNumber)
            {
                throw new InvalidDictionaryBaseNumberPairException("The dictionary could not be smaller than the base number.");
            }

            if (BaseNumber < 2)
            {
                throw new InvalidBaseNumberException("Base number should be greater than 1.");
            }
        }

        NumberBaseFormat BuildNew()
        {
            AssertBuilder();
            return new(DisplayName, Radix.Custom, BaseNumber, GroupSize, GroupSeparator, Dictionary);
        }

        Func<NumberBaseFormat> GetModifier(NumberBaseFormat _format)
        {
            return () =>
            {
                AssertBuilder();
                _format.Dictionary = Dictionary!;
                _format.BaseNumber = BaseNumber;
                return _format;
            };
        }

        internal static NumberBaseFormat BuildFormat(Action<NumberBaseFormatBuilder> configure)
        {
            var builder = new NumberBaseFormatBuilder();
            configure(builder);
            return builder.Build();
        }

        internal static NumberBaseFormat BuildFormat(NumberBaseFormat format, Action<NumberBaseFormatBuilder> configure)
        {
            var builder = new NumberBaseFormatBuilder(format);
            configure(builder);
            return builder.Build();
        }
    }
}

