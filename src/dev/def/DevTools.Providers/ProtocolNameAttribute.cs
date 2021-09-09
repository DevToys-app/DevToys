using System;
using System.Composition;
using System.Linq;

namespace DevTools.Providers
{
    /// <summary>
    /// Indicates the tool name used through URI Activation Protocol to access this <see cref="IToolProvider"/>.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProtocolNameAttribute : Attribute
    {
        public string ProtocolName { get; set; }

        public ProtocolNameAttribute(string protocolName)
        {
            if (string.IsNullOrWhiteSpace(protocolName)
                || !protocolName!.All(
                    c => char.IsDigit(c)
                         || (char.IsLetter(c) && char.IsLower(c))))
            {
                throw new ArgumentException("The protocol name should only have letters or digits and should be in lower case.");
            }

            ProtocolName = protocolName;
        }
    }
}
