#nullable enable

using System;
using System.Composition;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Indicates that the tool can not be added to the favorites.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class NotFavorableAttribute : Attribute
    {
        public bool NotFavorable { get; } = true;
    }
}
