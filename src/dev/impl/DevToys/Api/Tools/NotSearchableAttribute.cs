#nullable enable

using System;
using System.Composition;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Indicates that the tool can not be searched.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class NotSearchableAttribute : Attribute
    {
        public bool NotSearchable { get; } = true;
    }
}
