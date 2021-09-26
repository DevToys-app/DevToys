#nullable enable

using System;
using System.Composition;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Indicates an internal non-localized name.
    /// </summary>
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
