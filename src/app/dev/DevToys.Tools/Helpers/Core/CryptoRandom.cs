using System.Security.Cryptography;

namespace DevToys.Tools.Helpers.Core;

/// <summary>
/// A class that mimics the standard Random class in the .NET Framework - but uses a random number generator internally.
/// Taken from IdentityModel (ref.: https://github.com/IdentityModel/IdentityModel/ )
/// Taken from PasswordGenerator (ref.: https://github.com/Darkseal/PasswordGenerator/blob/master/CryptoRandom.cs )
/// </summary>
internal sealed class CryptoRandom : Random
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
    private readonly byte[] _uint32Buffer = new byte[4];

    /// <summary>
    /// Output format for unique IDs
    /// </summary>
    private enum OutputFormat
    {
        /// <summary>
        /// URL-safe Base64
        /// </summary>
        Base64Url,
        /// <summary>
        /// Base64
        /// </summary>
        Base64,
        /// <summary>
        /// Hex
        /// </summary>
        Hex
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoRandom"/> class.
    /// </summary>
    internal CryptoRandom()
    {
    }

    /// <summary>
    /// Returns a nonnegative random number.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>.
    /// </returns>
    public override int Next()
    {
        Rng.GetBytes(_uint32Buffer);
        return BitConverter.ToInt32(_uint32Buffer, 0) & 0x7FFFFFFF;
    }

    /// <summary>
    /// Returns a nonnegative random number less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to zero.</param>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily includes zero but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals zero, <paramref name="maxValue"/> is returned.
    /// </returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// 	<paramref name="maxValue"/> is less than zero.
    /// </exception>
    public override int Next(int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);

        return Next(0, maxValue);
    }

    /// <summary>
    /// Returns a random number within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but not <paramref name="maxValue"/>. If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
    /// </returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// 	<paramref name="minValue"/> is greater than <paramref name="maxValue"/>.
    /// </exception>
    public override int Next(int minValue, int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);

        if (minValue == maxValue)
            return minValue;

        long diff = maxValue - minValue;

        while (true)
        {
            Rng.GetBytes(_uint32Buffer);
            uint rand = BitConverter.ToUInt32(_uint32Buffer, 0);

            long max = 1 + (long)uint.MaxValue;
            long remainder = max % diff;
            if (rand < max - remainder)
                return (int)(minValue + rand % diff);
        }
    }

    /// <summary>
    /// Returns a random number between 0.0 and 1.0.
    /// </summary>
    /// <returns>
    /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
    /// </returns>
    public override double NextDouble()
    {
        Rng.GetBytes(_uint32Buffer);
        uint rand = BitConverter.ToUInt32(_uint32Buffer, 0);
        return rand / (1.0 + uint.MaxValue);
    }

    /// <summary>
    /// Fills the elements of a specified array of bytes with random numbers.
    /// </summary>
    /// <param name="buffer">An array of bytes to contain random numbers.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// 	<paramref name="buffer"/> is null.
    /// </exception>
    public override void NextBytes(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        Rng.GetBytes(buffer);
    }
}
