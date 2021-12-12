#nullable enable

using System;
using System.Composition;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Indicates that the tool does not support Compact Overlay mode.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class NoCompactOverlaySupportAttribute : Attribute
    {
        public bool NoCompactOverlaySupport { get; } = true;
    }
}

