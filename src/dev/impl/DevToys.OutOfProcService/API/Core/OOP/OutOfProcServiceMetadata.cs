using System.ComponentModel;
using DevToys.Shared.Core.OOP;

namespace DevToys.OutOfProcService.API.Core.OOP
{
    internal sealed class OutOfProcServiceMetadata
    {
        [DefaultValue(typeof(AppServiceMessageBase))]
        public Type InputType { get; set; } = typeof(AppServiceMessageBase);
    }
}
