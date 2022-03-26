#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.Messages
{
    public sealed class ChangeNumberFormattingMessage
    {
        public bool IsFormatted { get; set; }

        public ChangeNumberFormattingMessage(bool formatted)
        {
            IsFormatted = formatted;
        }
    }
}
