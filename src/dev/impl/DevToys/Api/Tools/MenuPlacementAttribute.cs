#nullable enable

using System;
using System.Composition;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Indicates where the <see cref="IToolProvider"/> should be displayed in the navigation view.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MenuPlacementAttribute : Attribute
    {
        public MenuPlacement MenuPlacement { get; }

        public MenuPlacementAttribute(MenuPlacement menuPlacement)
        {
            MenuPlacement = menuPlacement;
        }
    }
}
