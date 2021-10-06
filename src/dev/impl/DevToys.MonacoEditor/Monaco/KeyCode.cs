#nullable enable

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// Virtual Key Codes, the value does not hold any inherent meaning.
    /// Inspired somewhat from https://msdn.microsoft.com/en-us/library/windows/desktop/dd375731(v=vs.85).aspx
    /// But these are "more general", as they should work across browsers &amp; OS`s.
    /// </summary>
    internal static class KeyCode
    {
        /**
         * Placed first to cover the 0 value of the enum.
         */
        internal const int Unknown = 0;
        internal const int Backspace = 1;
        internal const int Tab = 2;
        internal const int Enter = 3;
        internal const int Shift = 4;
        internal const int Ctrl = 5;
        internal const int Alt = 6;
        internal const int PauseBreak = 7;
        internal const int CapsLock = 8;
        internal const int Escape = 9;
        internal const int Space = 10;
        internal const int PageUp = 11;
        internal const int PageDown = 12;
        internal const int End = 13;
        internal const int Home = 14;
        internal const int LeftArrow = 15;
        internal const int UpArrow = 16;
        internal const int RightArrow = 17;
        internal const int DownArrow = 18;
        internal const int Insert = 19;
        internal const int Delete = 20;
        internal const int KEY_0 = 21;
        internal const int KEY_1 = 22;
        internal const int KEY_2 = 23;
        internal const int KEY_3 = 24;
        internal const int KEY_4 = 25;
        internal const int KEY_5 = 26;
        internal const int KEY_6 = 27;
        internal const int KEY_7 = 28;
        internal const int KEY_8 = 29;
        internal const int KEY_9 = 30;
        internal const int KEY_A = 31;
        internal const int KEY_B = 32;
        internal const int KEY_C = 33;
        internal const int KEY_D = 34;
        internal const int KEY_E = 35;
        internal const int KEY_F = 36;
        internal const int KEY_G = 37;
        internal const int KEY_H = 38;
        internal const int KEY_I = 39;
        internal const int KEY_J = 40;
        internal const int KEY_K = 41;
        internal const int KEY_L = 42;
        internal const int KEY_M = 43;
        internal const int KEY_N = 44;
        internal const int KEY_O = 45;
        internal const int KEY_P = 46;
        internal const int KEY_Q = 47;
        internal const int KEY_R = 48;
        internal const int KEY_S = 49;
        internal const int KEY_T = 50;
        internal const int KEY_U = 51;
        internal const int KEY_V = 52;
        internal const int KEY_W = 53;
        internal const int KEY_X = 54;
        internal const int KEY_Y = 55;
        internal const int KEY_Z = 56;
        internal const int Meta = 57;
        internal const int ContextMenu = 58;
        internal const int F1 = 59;
        internal const int F2 = 60;
        internal const int F3 = 61;
        internal const int F4 = 62;
        internal const int F5 = 63;
        internal const int F6 = 64;
        internal const int F7 = 65;
        internal const int F8 = 66;
        internal const int F9 = 67;
        internal const int F10 = 68;
        internal const int F11 = 69;
        internal const int F12 = 70;
        internal const int F13 = 71;
        internal const int F14 = 72;
        internal const int F15 = 73;
        internal const int F16 = 74;
        internal const int F17 = 75;
        internal const int F18 = 76;
        internal const int F19 = 77;
        internal const int NumLock = 78;
        internal const int ScrollLock = 79;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the ';:' key
         */
        internal const int US_SEMICOLON = 80;
        /**
         * For any country/region, the '+' key
         * For the US standard keyboard, the '=+' key
         */
        internal const int US_EQUAL = 81;
        /**
         * For any country/region, the ',' key
         * For the US standard keyboard, the ',&lt;' key
         */
        internal const int US_COMMA = 82;
        /**
         * For any country/region, the '-' key
         * For the US standard keyboard, the '-_' key
         */
        internal const int US_MINUS = 83;
        /**
         * For any country/region, the '.' key
         * For the US standard keyboard, the '.>' key
         */
        internal const int US_DOT = 84;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the '/?' key
         */
        internal const int US_SLASH = 85;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the '`~' key
         */
        internal const int US_BACKTICK = 86;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the '[{' key
         */
        internal const int US_OPEN_SQUARE_BRACKET = 87;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the '\|' key
         */
        internal const int US_BACKSLASH = 88;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the ']}' key
         */
        internal const int US_CLOSE_SQUARE_BRACKET = 89;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the ''"' key
         */
        internal const int US_QUOTE = 90;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         */
        internal const int OEM_8 = 91;
        /**
         * Either the angle bracket key or the backslash key on the RT 102-key keyboard.
         */
        internal const int OEM_102 = 92;
        internal const int NUMPAD_0 = 93;
        internal const int NUMPAD_1 = 94;
        internal const int NUMPAD_2 = 95;
        internal const int NUMPAD_3 = 96;
        internal const int NUMPAD_4 = 97;
        internal const int NUMPAD_5 = 98;
        internal const int NUMPAD_6 = 99;
        internal const int NUMPAD_7 = 100;
        internal const int NUMPAD_8 = 101;
        internal const int NUMPAD_9 = 102;
        internal const int NUMPAD_MULTIPLY = 103;
        internal const int NUMPAD_ADD = 104;
        internal const int NUMPAD_SEPARATOR = 105;
        internal const int NUMPAD_SUBTRACT = 106;
        internal const int NUMPAD_DECIMAL = 107;
        internal const int NUMPAD_DIVIDE = 108;
        /**
         * Cover all key codes when IME is processing input.
         */
        internal const int KEY_IN_COMPOSITION = 109;
        internal const int ABNT_C1 = 110;
        internal const int ABNT_C2 = 111;
        /**
         * Placed last to cover the length of the enum.
         * Please do not depend on this value!
         */
        internal const int MAX_VALUE = 112;
    }
}
