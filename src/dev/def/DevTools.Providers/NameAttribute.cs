using System;
using System.Composition;

namespace DevTools.Providers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class NameAttribute : Attribute
    {
        public string? Name { get; set; }

        public NameAttribute(string? name)
        {
            Name = name;
        }
    }
}
