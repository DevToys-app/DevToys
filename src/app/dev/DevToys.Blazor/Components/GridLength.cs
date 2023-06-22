namespace DevToys.Blazor.Components;

public readonly struct GridLength : IEquatable<GridLength>
{
    private readonly GridUnitType _type;

    private readonly double _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="GridLength"/> struct.
    /// </summary>
    /// <param name="value">The size of the GridLength in device independent pixels.</param>
    public GridLength(double value)
        : this(value, GridUnitType.Pixel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GridLength"/> struct.
    /// </summary>
    /// <param name="value">The size of the <see cref="GridLength"/>.</param>
    /// <param name="type">The unit of the <see cref="GridLength"/>.</param>
    public GridLength(double value, GridUnitType type)
    {
        if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Invalid value", nameof(value));
        }

        if (type < GridUnitType.Auto || type > GridUnitType.Fraction)
        {
            throw new ArgumentException("Invalid value", nameof(type));
        }

        _type = type;
        _value = value;
    }

    /// <summary>
    /// Gets an instance of <see cref="GridLength"/> that indicates that a row or column should
    /// auto-size to fit its content.
    /// </summary>
    public static GridLength Auto => new(0, GridUnitType.Auto);

    /// <summary>
    /// Gets the unit of the <see cref="GridLength"/>.
    /// </summary>
    public GridUnitType GridUnitType => _type;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="GridLength"/> has a <see cref="GridUnitType"/> of Pixel.
    /// </summary>
    public bool IsAbsolute => _type == GridUnitType.Pixel;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="GridLength"/> has a <see cref="GridUnitType"/> of Auto.
    /// </summary>
    public bool IsAuto => _type == GridUnitType.Auto;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="GridLength"/> has a <see cref="GridUnitType"/> of Fraction.
    /// </summary>
    public bool IsFraction => _type == GridUnitType.Fraction;

    /// <summary>
    /// Gets the length.
    /// </summary>
    public double Value => _value;

    /// <summary>
    /// Compares two <see cref="GridLength"/> structures for equality.
    /// </summary>
    /// <param name="a">The first <see cref="GridLength"/>.</param>
    /// <param name="b">The second <see cref="GridLength"/>.</param>
    /// <returns>True if the structures are equal, otherwise false.</returns>
    public static bool operator ==(GridLength a, GridLength b)
    {
        return (a.IsAuto && b.IsAuto) || (a._value == b._value && a._type == b._type);
    }

    /// <summary>
    /// Compares two <see cref="GridLength"/> structures for inequality.
    /// </summary>
    /// <param name="gl1">The first <see cref="GridLength"/>.</param>
    /// <param name="gl2">The first <see cref="GridLength"/>.</param>
    /// <returns>True if the structures are unequal, otherwise false.</returns>
    public static bool operator !=(GridLength gl1, GridLength gl2)
    {
        return !(gl1 == gl2);
    }

    /// <summary>
    /// Determines whether the <see cref="GridLength"/> is equal to the specified object.
    /// </summary>
    /// <param name="o">The object with which to test equality.</param>
    /// <returns>True if the objects are equal, otherwise false.</returns>
    public override bool Equals(object? o)
    {
        if (o == null)
        {
            return false;
        }

        if (o is not GridLength)
        {
            return false;
        }

        return this == (GridLength)o;
    }

    /// <summary>
    /// Compares two <see cref="GridLength"/> structures for equality.
    /// </summary>
    /// <param name="gridLength">The structure with which to test equality.</param>
    /// <returns>True if the structures are equal, otherwise false.</returns>
    public bool Equals(GridLength gridLength)
    {
        return this == gridLength;
    }

    /// <summary>
    /// Gets a hash code for the <see cref="GridLength"/>.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return _value.GetHashCode() ^ _type.GetHashCode();
    }
}
