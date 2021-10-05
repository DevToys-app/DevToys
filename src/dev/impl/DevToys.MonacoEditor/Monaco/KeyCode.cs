#nullable enable

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// Virtual Key Codes, the value does not hold any inherent meaning.
    /// Inspired somewhat from https://msdn.microsoft.com/en-us/library/windows/desktop/dd375731(v=vs.85).aspx
    /// But these are "more general", as they should work across browsers &amp; OS`s.
    /// </summary>
    public static class KeyCode
    {
        /**
         * Placed first to cover the 0 value of the enum.
         */
        public const int Unknown = 0;
        public const int Backspace = 1;
        public const int Tab = 2;
        public const int Enter = 3;
        public const int Shift = 4;
        public const int Ctrl = 5;
        public const int Alt = 6;
        public const int PauseBreak = 7;
        public const int CapsLock = 8;
        public const int Escape = 9;
        public const int Space = 10;
        public const int PageUp = 11;
        public const int PageDown = 12;
        public const int End = 13;
        public const int Home = 14;
        public const int LeftArrow = 15;
        public const int UpArrow = 16;
        public const int RightArrow = 17;
        public const int DownArrow = 18;
        public const int Insert = 19;
        public const int Delete = 20;
        public const int KEY_0 = 21;
        public const int KEY_1 = 22;
        public const int KEY_2 = 23;
        public const int KEY_3 = 24;
        public const int KEY_4 = 25;
        public const int KEY_5 = 26;
        public const int KEY_6 = 27;
        public const int KEY_7 = 28;
        public const int KEY_8 = 29;
        public const int KEY_9 = 30;
        public const int KEY_A = 31;
        public const int KEY_B = 32;
        public const int KEY_C = 33;
        public const int KEY_D = 34;
        public const int KEY_E = 35;
        public const int KEY_F = 36;
        public const int KEY_G = 37;
        public const int KEY_H = 38;
        public const int KEY_I = 39;
        public const int KEY_J = 40;
        public const int KEY_K = 41;
        public const int KEY_L = 42;
        public const int KEY_M = 43;
        public const int KEY_N = 44;
        public const int KEY_O = 45;
        public const int KEY_P = 46;
        public const int KEY_Q = 47;
        public const int KEY_R = 48;
        public const int KEY_S = 49;
        public const int KEY_T = 50;
        public const int KEY_U = 51;
        public const int KEY_V = 52;
        public const int KEY_W = 53;
        public const int KEY_X = 54;
        public const int KEY_Y = 55;
        public const int KEY_Z = 56;
        public const int Meta = 57;
        public const int ContextMenu = 58;
        public const int F1 = 59;
        public const int F2 = 60;
        public const int F3 = 61;
        public const int F4 = 62;
        public const int F5 = 63;
        public const int F6 = 64;
        public const int F7 = 65;
        public const int F8 = 66;
        public const int F9 = 67;
        public const int F10 = 68;
        public const int F11 = 69;
        public const int F12 = 70;
        public const int F13 = 71;
        public const int F14 = 72;
        public const int F15 = 73;
        public const int F16 = 74;
        public const int F17 = 75;
        public const int F18 = 76;
        public const int F19 = 77;
        public const int NumLock = 78;
        public const int ScrollLock = 79;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the ';:' key
         */
        public const int US_SEMICOLON = 80;
        /**
         * For any country/region, the '+' key
         * For the US standard keyboard, the '=+' key
         */
        public const int US_EQUAL = 81;
        /**
         * For any country/region, the ',' key
         * For the US standard keyboard, the ',&lt;' key
         */
        public const int US_COMMA = 82;
        /**
         * For any country/region, the '-' key
         * For the US standard keyboard, the '-_' key
         */
        public const int US_MINUS = 83;
        /**
         * For any country/region, the '.' key
         * For the US standard keyboard, the '.>' key
         */
        public const int US_DOT = 84;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the '/?' key
         */
        public const int US_SLASH = 85;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the '`~' key
         */
        public const int US_BACKTICK = 86;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the '[{' key
         */
        public const int US_OPEN_SQUARE_BRACKET = 87;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the '\|' key
         */
        public const int US_BACKSLASH = 88;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the ']}' key
         */
        public const int US_CLOSE_SQUARE_BRACKET = 89;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         * For the US standard keyboard, the ''"' key
         */
        public const int US_QUOTE = 90;
        /**
         * Used for miscellaneous characters; it can vary by keyboard.
         */
        public const int OEM_8 = 91;
        /**
         * Either the angle bracket key or the backslash key on the RT 102-key keyboard.
         */
        public const int OEM_102 = 92;
        public const int NUMPAD_0 = 93;
        public const int NUMPAD_1 = 94;
        public const int NUMPAD_2 = 95;
        public const int NUMPAD_3 = 96;
        public const int NUMPAD_4 = 97;
        public const int NUMPAD_5 = 98;
        public const int NUMPAD_6 = 99;
        public const int NUMPAD_7 = 100;
        public const int NUMPAD_8 = 101;
        public const int NUMPAD_9 = 102;
        public const int NUMPAD_MULTIPLY = 103;
        public const int NUMPAD_ADD = 104;
        public const int NUMPAD_SEPARATOR = 105;
        public const int NUMPAD_SUBTRACT = 106;
        public const int NUMPAD_DECIMAL = 107;
        public const int NUMPAD_DIVIDE = 108;
        /**
         * Cover all key codes when IME is processing input.
         */
        public const int KEY_IN_COMPOSITION = 109;
        public const int ABNT_C1 = 110;
        public const int ABNT_C2 = 111;
        /**
         * Placed last to cover the length of the enum.
         * Please do not depend on this value!
         */
        public const int MAX_VALUE = 112;
    }
}
