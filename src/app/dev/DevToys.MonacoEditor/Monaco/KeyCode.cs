namespace DevToys.MonacoEditor.Monaco;

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
    public static int Unknown => 0;
    public static int Backspace => 1;
    public static int Tab => 2;
    public static int Enter => 3;
    public static int Shift => 4;
    public static int Ctrl => 5;
    public static int Alt => 6;
    public static int PauseBreak => 7;
    public static int CapsLock => 8;
    public static int Escape => 9;
    public static int Space => 10;
    public static int PageUp => 11;
    public static int PageDown => 12;
    public static int End => 13;
    public static int Home => 14;
    public static int LeftArrow => 15;
    public static int UpArrow => 16;
    public static int RightArrow => 17;
    public static int DownArrow => 18;
    public static int Insert => 19;
    public static int Delete => 20;
    public static int KEY_0 => 21;
    public static int KEY_1 => 22;
    public static int KEY_2 => 23;
    public static int KEY_3 => 24;
    public static int KEY_4 => 25;
    public static int KEY_5 => 26;
    public static int KEY_6 => 27;
    public static int KEY_7 => 28;
    public static int KEY_8 => 29;
    public static int KEY_9 => 30;
    public static int KEY_A => 31;
    public static int KEY_B => 32;
    public static int KEY_C => 33;
    public static int KEY_D => 34;
    public static int KEY_E => 35;
    public static int KEY_F => 36;
    public static int KEY_G => 37;
    public static int KEY_H => 38;
    public static int KEY_I => 39;
    public static int KEY_J => 40;
    public static int KEY_K => 41;
    public static int KEY_L => 42;
    public static int KEY_M => 43;
    public static int KEY_N => 44;
    public static int KEY_O => 45;
    public static int KEY_P => 46;
    public static int KEY_Q => 47;
    public static int KEY_R => 48;
    public static int KEY_S => 49;
    public static int KEY_T => 50;
    public static int KEY_U => 51;
    public static int KEY_V => 52;
    public static int KEY_W => 53;
    public static int KEY_X => 54;
    public static int KEY_Y => 55;
    public static int KEY_Z => 56;
    public static int Meta => 57;
    public static int ContextMenu => 58;
    public static int F1 => 59;
    public static int F2 => 60;
    public static int F3 => 61;
    public static int F4 => 62;
    public static int F5 => 63;
    public static int F6 => 64;
    public static int F7 => 65;
    public static int F8 => 66;
    public static int F9 => 67;
    public static int F10 => 68;
    public static int F11 => 69;
    public static int F12 => 70;
    public static int F13 => 71;
    public static int F14 => 72;
    public static int F15 => 73;
    public static int F16 => 74;
    public static int F17 => 75;
    public static int F18 => 76;
    public static int F19 => 77;
    public static int NumLock => 78;
    public static int ScrollLock => 79;
    /**
     * Used for miscellaneous characters; it can vary by keyboard.
     * For the US standard keyboard, the ';:' key
     */
    public static int US_SEMICOLON => 80;
    /**
     * For any country/region, the '+' key
     * For the US standard keyboard, the '=+' key
     */
    public static int US_EQUAL => 81;
    /**
     * For any country/region, the ',' key
     * For the US standard keyboard, the ',&lt;' key
     */
    public static int US_COMMA => 82;
    /**
     * For any country/region, the '-' key
     * For the US standard keyboard, the '-_' key
     */
    public static int US_MINUS => 83;
    /**
     * For any country/region, the '.' key
     * For the US standard keyboard, the '.>' key
     */
    public static int US_DOT => 84;
    /**
     * Used for miscellaneous characters; it can vary by keyboard.
     * For the US standard keyboard, the '/?' key
     */
    public static int US_SLASH => 85;
    /**
     * Used for miscellaneous characters; it can vary by keyboard.
     * For the US standard keyboard, the '`~' key
     */
    public static int US_BACKTICK => 86;
    /**
     * Used for miscellaneous characters; it can vary by keyboard.
     * For the US standard keyboard, the '[{' key
     */
    public static int US_OPEN_SQUARE_BRACKET => 87;
    /**
     * Used for miscellaneous characters; it can vary by keyboard.
     * For the US standard keyboard, the '\|' key
     */
    public static int US_BACKSLASH => 88;
    /**
     * Used for miscellaneous characters; it can vary by keyboard.
     * For the US standard keyboard, the ']}' key
     */
    public static int US_CLOSE_SQUARE_BRACKET => 89;
    /**
     * Used for miscellaneous characters; it can vary by keyboard.
     * For the US standard keyboard, the ''"' key
     */
    public static int US_QUOTE => 90;
    /**
     * Used for miscellaneous characters; it can vary by keyboard.
     */
    public static int OEM_8 => 91;
    /**
     * Either the angle bracket key or the backslash key on the RT 102-key keyboard.
     */
    public static int OEM_102 => 92;
    public static int NUMPAD_0 => 93;
    public static int NUMPAD_1 => 94;
    public static int NUMPAD_2 => 95;
    public static int NUMPAD_3 => 96;
    public static int NUMPAD_4 => 97;
    public static int NUMPAD_5 => 98;
    public static int NUMPAD_6 => 99;
    public static int NUMPAD_7 => 100;
    public static int NUMPAD_8 => 101;
    public static int NUMPAD_9 => 102;
    public static int NUMPAD_MULTIPLY => 103;
    public static int NUMPAD_ADD => 104;
    public static int NUMPAD_SEPARATOR => 105;
    public static int NUMPAD_SUBTRACT => 106;
    public static int NUMPAD_DECIMAL => 107;
    public static int NUMPAD_DIVIDE => 108;
    /**
     * Cover all key codes when IME is processing input.
     */
    public static int KEY_IN_COMPOSITION => 109;
    public static int ABNT_C1 => 110;
    public static int ABNT_C2 => 111;
    /**
     * Placed last to cover the length of the enum.
     * Please do not depend on this value!
     */
    public static int MAX_VALUE => 112;
}
