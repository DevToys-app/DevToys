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

        public ChangeInfoBarStatusMessage(string message)
        {
            Message = message;
        }
    }
}
