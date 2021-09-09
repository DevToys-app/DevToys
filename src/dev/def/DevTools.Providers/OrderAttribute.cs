using System;
using System.Composition;

namespace DevTools.Providers
{
    /// <summary>
    /// Indicates the order in which this <see cref="IToolProvider"/> should appear.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class OrderAttribute : Attribute
    {
        public int? Order { get; set; }

        public OrderAttribute(int order)
        {
            Order = order;
        }
    }
}
