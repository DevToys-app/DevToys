using System;
using System.Composition;

namespace DevTools.Providers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class IsFooterItemAttribute : Attribute
    {
        public bool IsFooterItem { get; set; } = true;
    }
}
