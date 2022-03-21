#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.Messages
{
    public sealed class ChangeInfoBarStatusMessage
    {
        public string Message { get; set; }
        public bool IsOpen { get; set; }

        public ChangeInfoBarStatusMessage(bool isOpen, string message)
        {
            Message = message;
            IsOpen = isOpen;
        }
    }
}
