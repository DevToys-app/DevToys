using System;
using System.Composition;

namespace DevTools.Providers
{
    /// <summary>
    /// Indicates that the tool view can not be scrolled.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class NotScrollableAttribute : Attribute
    {
        public bool NotScrollable { get; } = true;
    }
}
