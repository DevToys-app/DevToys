#nullable enable

using System;
using System.Composition;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Indicates the parent tool of the current one.
    /// The name should corresponds to an existing <see cref="NameAttribute.Name"/> value, or null if no parent.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ParentAttribute : Attribute
    {
        public string Parent { get; set; }

        public ParentAttribute(string? name)
        {
            Parent = name ?? string.Empty;
        }
    }
}
