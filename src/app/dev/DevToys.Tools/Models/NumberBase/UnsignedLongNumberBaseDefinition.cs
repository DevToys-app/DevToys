using System.Text;

namespace DevToys.Tools.Models.NumberBase;

internal abstract class UnsignedLongNumberBaseDefinition : INumberBaseDefinition<ulong>
{
    private readonly string _dictionary;
    private readonly ulong _base;
    private readonly bool _isDictionaryCaseSensitive;

    protected UnsignedLongNumberBaseDefinition(
        string displayName,
        string dictionary,
        bool isDictionaryCaseSensitive)
    {
        Guard.IsNotNullOrEmpty(dictionary);
        Guard.HasSizeGreaterThanOrEqualTo(dictionary, 2);
        Guard.IsNotNullOrWhiteSpace(displayName);
        _dictionary = dictionary!;
        _base = (ulong)dictionary.Length;
        _isDictionaryCaseSensitive = isDictionaryCaseSensitive;
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public ulong MaxValue => ulong.MaxValue / _base;

    public string ToFormattedString(ulong number, bool format)
    {
        // If the number is zero, return the first character of the dictionary as a string
        if (number == 0)
        {
            return _dictionary[0].ToString();
        }

        var result = new StringBuilder();

        // Convert the number to a string representation using the dictionary
        // by repeatedly dividing the number by the base and prepending the character
        // at the index of the remainder in the dictionary to the result
        while (number > 0)
        {
            ulong remainder = number % _base;
            result.Insert(0, _dictionary[(int)remainder]);
            number /= _base;
        }

        string numberString = result.ToString();

        // If no formatting is required, return the string representation
        if (!format)
        {
            return numberString;
        }

        // If formatting is required, insert a space every four characters
        // starting from the end of the string
        var formattedNumber = new StringBuilder();
        int count = 0;

        for (int i = numberString.Length - 1; i >= 0; i--)
        {
            if (count > 0 && count % 4 == 0)
            {
                formattedNumber.Insert(0, ' ');
            }

            formattedNumber.Insert(0, numberString[i]);
            count++;
        }

        // Return the formatted string
        return formattedNumber.ToString();
    }

    public ulong Parse(string number)
    {
        // Let's use a unsigned long to store the result. Unsigned numbers are always positive.
        ulong numericValue = 0;

        // Iterate over each character in the input string
        for (int i = 0; i < number.Length; i++)
        {
            // Check if the current character is valid
            char currentCharacter = number[i];
            if (!IsCharacterValid(currentCharacter, out int indexOfCurrentCharacterInDictionary))
            {
                throw new InvalidOperationException();
            }

            if (numericValue > MaxValue)
            {
                throw new OverflowException();
            }

            // Calculate the new result. This is done by multiplying the current result by the base and adding the value of the current character.
            // This is equivalent to shifting the current result left by the number of bits required to represent the base, and then adding the value of the current character.
            ulong newNumericValue = numericValue * _base + (ulong)indexOfCurrentCharacterInDictionary;

            // Check if the new result is less than the old result (which would indicate an overflow)
            if (newNumericValue < numericValue)
            {
                throw new OverflowException();
            }

            // Update the result
            numericValue = newNumericValue;
        }

        return numericValue;
    }

    private bool IsCharacterValid(char c, out int number)
    {
        if (!_isDictionaryCaseSensitive)
        {
            c = char.ToLowerInvariant(c);
        }

        for (number = 0; number < _dictionary.Length; number++)
        {
            char dictionaryChar = _dictionary[number];
            if (!_isDictionaryCaseSensitive)
            {
                dictionaryChar = char.ToLowerInvariant(dictionaryChar);
            }

            if (dictionaryChar == c)
            {
                return true;
            }
        }

        number = -1;
        return false;
    }
}
