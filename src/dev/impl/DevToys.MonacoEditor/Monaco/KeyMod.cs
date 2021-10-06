#nullable enable

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// https://microsoft.github.io/monaco-editor/api/classes/monaco.keymod.html
    /// </summary>
    public sealed class KeyMod
    {
        internal const int WinCtrl = 256;

        internal const int Alt = 512;

        internal const int Shift = 1024;

        internal const int CtrlCmd = 2048;

        public static int Chord(int firstPart, int secondPart)
        {
            // https://github.com/Microsoft/vscode/blob/master/src/vs/base/common/keyCodes.ts#L410
            var chordPart = ZeroFillRightShift((secondPart & 0x0000ffff) << 16, 0);
            return ZeroFillRightShift(firstPart | chordPart, 0);
        }

        // Info on Zero-Fill Right Shift http://www.vanguardsw.com/dphelp4/dph00369.htm
        // Supported natively by JavaScript, but not C#
        private static int ZeroFillRightShift(int i, int j)
        {
            bool negativemask = i < 0;
            i >>= j;

            if (negativemask)
            {
                i &= 0x7FFFFFFF;
            }

            return i;
        }
    }
}
