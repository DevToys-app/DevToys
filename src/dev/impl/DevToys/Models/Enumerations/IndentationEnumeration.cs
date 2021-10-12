using DevToys.Core;
using System.Collections.ObjectModel;

namespace DevToys.Models.Enumerations
{
    public class IndentationEnumeration : Enumeration
    {
        private static JsonFormatterStrings Strings => LanguageManager.Instance.JsonFormatter;

        public static IndentationEnumeration TwoSpaceIndentation = new IndentationEnumeration(Strings.TwoSpaces, "TwoSpaces");

        public static IndentationEnumeration FourSpaceIndentation = new IndentationEnumeration(Strings.FourSpaces, "FourSpaces");

        public static IndentationEnumeration OneTabIndentation = new IndentationEnumeration(Strings.OneTab, "OneTab");

        public static IndentationEnumeration NoIndentation = new IndentationEnumeration(Strings.Minified, "Minified");

        internal IndentationEnumeration(string name, string code)
            : base(name, code)
        { }

        internal static ObservableCollection<IndentationEnumeration> Gets()
        {
            return new ObservableCollection<IndentationEnumeration>
            {
                TwoSpaceIndentation, FourSpaceIndentation, OneTabIndentation, NoIndentation
            };
        }
    }
}
