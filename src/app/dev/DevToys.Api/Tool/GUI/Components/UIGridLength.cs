namespace DevToys.Api;

/// <summary>
/// Represents the length of a row or column.
/// </summary>
[DebuggerDisplay($"{{{nameof(ToString)}}}")]
public readonly struct UIGridLength : IEquatable<UIGridLength>
{
    private readonly UIGridUnitType _type;

    private readonly double _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIGridLength"/> struct.
    /// </summary>
    /// <param name="value">The size of the UIGridLength in device independent pixels.</param>
    public UIGridLength(double value)
        : this(value, UIGridUnitType.Pixel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UIGridLength"/> struct.
    /// </summary>
    /// <param name="value">The size of the <see cref="UIGridLength"/>.</param>
    /// <param name="type">The unit of the <see cref="UIGridLength"/>.</param>
    public UIGridLength(double value, UIGridUnitType type)
    {
        if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Invalid value", nameof(value));
        }

        if (type < UIGridUnitType.Auto || type > UIGridUnitType.Fraction)
        {
            throw new ArgumentException("Invalid value", nameof(type));
        }

        _type = type;
        _value = value;
    }

    /// <summary>
    /// Gets an instance of <see cref="UIGridLength"/> that indicates that a row or column should
    /// auto-size to fit its content.
    /// </summary>
    public static UIGridLength Auto => new(0, UIGridUnitType.Auto);

    /// <summary>
    /// Gets the unit of the <see cref="UIGridLength"/>.
    /// </summary>
    public UIGridUnitType UIGridUnitType => _type;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="UIGridLength"/> has a <see cref="UIGridUnitType"/> of Pixel.
    /// </summary>
    public bool IsAbsolute => _type == UIGridUnitType.Pixel;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="UIGridLength"/> has a <see cref="UIGridUnitType"/> of Auto.
    /// </summary>
    public bool IsAuto => _type == UIGridUnitType.Auto;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="UIGridLength"/> has a <see cref="UIGridUnitType"/> of Fraction.
    /// </summary>
    public bool IsFraction => _type == UIGridUnitType.Fraction;

    /// <summary>
    /// Gets the length.
    /// </summary>
    public double Value => _value;

    /// <summary>
    /// Creates a <see cref="UIGridLength"/> from an number interpreted as a pixels.
    /// </summary>
    public static implicit operator UIGridLength(double pixels)
        => new(pixels, UIGridUnitType.Pixel);

    /// <summary>
    /// Compares two <see cref="UIGridLength"/> structures for equality.
    /// </summary>
    /// <param name="a">The first <see cref="UIGridLength"/>.</param>
    /// <param name="b">The second <see cref="UIGridLength"/>.</param>
    /// <returns>True if the structures are equal, otherwise false.</returns>
    public static bool operator ==(UIGridLength a, UIGridLength b)
    {
        return (a.IsAuto && b.IsAuto) || (a._value == b._value && a._type == b._type);
    }

    /// <summary>
    /// Compares two <see cref="UIGridLength"/> structures for inequality.
    /// </summary>
    /// <param name="gl1">The first <see cref="UIGridLength"/>.</param>
    /// <param name="gl2">The first <see cref="UIGridLength"/>.</param>
    /// <returns>True if the structures are unequal, otherwise false.</returns>
    public static bool operator !=(UIGridLength gl1, UIGridLength gl2)
    {
        return !(gl1 == gl2);
    }

    /// <summary>
    /// Determines whether the <see cref="UIGridLength"/> is equal to the specified object.
    /// </summary>
    /// <param name="o">The object with which to test equality.</param>
    /// <returns>True if the objects are equal, otherwise false.</returns>
    public override bool Equals(object? o)
    {
        if (o == null)
        {
            return false;
        }

        if (o is not UIGridLength)
        {
            return false;
        }

        return this == (UIGridLength)o;
    }

    /// <summary>
    /// Compares two <see cref="UIGridLength"/> structures for equality.
    /// </summary>
    /// <param name="gridLength">The structure with which to test equality.</param>
    /// <returns>True if the structures are equal, otherwise false.</returns>
    public bool Equals(UIGridLength gridLength)
    {
        return this == gridLength;
    }

    /// <summary>
    /// Gets a hash code for the <see cref="UIGridLength"/>.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return _value.GetHashCode() ^ _type.GetHashCode();
    }

    /// <summary>
    /// Returns a string that represents the current <see cref="UIGridLength"/>.
    /// </summary>
    /// <returns>A string that represents the current <see cref="UIGridLength"/>.</returns>
    public override string ToString()
    {
        if (IsAuto)
        {
            return "Auto";
        }

        if (IsAbsolute)
        {
            return $"{Value}px";
        }

        return $"{Value}*";
    }
}
