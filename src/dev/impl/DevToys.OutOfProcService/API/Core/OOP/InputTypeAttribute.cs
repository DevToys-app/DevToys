using System;
using System.Composition;
using System.IO;
using DevToys.Shared.Core.OOP;

namespace DevToys.OutOfProcService.API.Core.OOP
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class InputTypeAttribute : Attribute
    {
        public Type? InputType { get; set; }

        public InputTypeAttribute(Type type)
        {
            InputType = type;
            if (!type.IsSubclassOf(typeof(AppServiceMessageBase)))
            {
                throw new InvalidDataException();
            }
        }
    }
}
