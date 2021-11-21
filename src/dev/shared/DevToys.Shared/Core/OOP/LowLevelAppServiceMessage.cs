#nullable enable

using System.Text;

namespace DevToys.Shared.Core.OOP
{
    internal class LowLevelAppServiceMessage
    {
        internal byte[] Buffer { get; }

        internal StringBuilder Message { get; }

        internal LowLevelAppServiceMessage(int bufferSize, StringBuilder? stringBuilder)
        {
            Buffer = new byte[bufferSize];
            Message = stringBuilder ?? new StringBuilder();
        }
    }
}
