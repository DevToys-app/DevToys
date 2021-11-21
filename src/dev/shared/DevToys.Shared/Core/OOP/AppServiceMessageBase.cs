#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.Shared.Core.OOP
{
    public abstract class AppServiceMessageBase
    {
        [JsonProperty]
        internal Guid? MessageId { get; set; }
    }
}
