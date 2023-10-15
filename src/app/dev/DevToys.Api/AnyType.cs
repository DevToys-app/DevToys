using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DevToys.Api;

public static class AnyTypeIdentifiers
{
    public const string AnyTypeT2GuidString = "a6a7b494-61f1-4826-ba9a-bb5c569817de";
    public const string AnyTypeT3GuidString = "59f7ac53-30bd-4066-879a-cbd1672ef5fb";
    public const string AnyTypeT4GuidString = "3987d3c2-94d0-4818-ae49-9036c60affe7";
    public static readonly Guid AnyTypeT2Guid = new Guid(AnyTypeT2GuidString);
    public static readonly Guid AnyTypeT3Guid = new Guid(AnyTypeT3GuidString);
    public static readonly Guid AnyTypeT4Guid = new Guid(AnyTypeT4GuidString);
}

[Guid(AnyTypeIdentifiers.AnyTypeT2GuidString)]
public readonly struct AnyType<T1, T2> : IEquatable<AnyType<T1, T2>>
{
    public AnyType(T1 val)
    {
        Value = val;
    }

    public AnyType(T2 val)
    {
        Value = val;
    }

    public object? Value { get; }

    public readonly T1 First => (T1)this;

    public readonly T2 Second => (T2)this;

    public readonly bool TryGetFirst([MaybeNullWhen(false)] out T1 value)
    {
        if (Value is T1 obj)
        {
            value = obj;
            return true;
        }

        value = default;
        return false;
    }

    public readonly bool TryGetSecond([MaybeNullWhen(false)] out T2 value)
    {
        if (Value is T2 obj)
        {
            value = obj;
            return true;
        }

        value = default;
        return false;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is AnyType<T1, T2> other && Equals(other);
    }

    public readonly bool Equals(AnyType<T1, T2> other)
    {
        return EqualityComparer<object>.Default.Equals(Value, other.Value);
    }

    public override readonly int GetHashCode()
    {
        if (Value is null)
        {
            return 0;
        }

        return EqualityComparer<object>.Default.GetHashCode(Value);
    }

    public static bool operator ==(AnyType<T1, T2> left, AnyType<T1, T2> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AnyType<T1, T2> left, AnyType<T1, T2> right)
    {
        return !(left == right);
    }

    public static implicit operator AnyType<T1, T2>(T1 val)
    {
        return new(val);
    }

    public static implicit operator AnyType<T1, T2>?(T1? val)
    {
        return val != null ? new AnyType<T1, T2>?(new AnyType<T1, T2>(val)) : new AnyType<T1, T2>?();
    }

    public static implicit operator AnyType<T1, T2>(T2 val)
    {
        return new(val);
    }

    public static implicit operator AnyType<T1, T2>?(T2? val)
    {
        return val != null ? new AnyType<T1, T2>?(new AnyType<T1, T2>(val)) : new AnyType<T1, T2>?();
    }

    public static explicit operator T1(AnyType<T1, T2> sum)
    {
        if (sum.Value is T1 obj)
        {
            return obj;
        }

        throw new InvalidCastException();
    }

    public static explicit operator T2(AnyType<T1, T2> sum)
    {
        if (sum.Value is T2 obj)
        {
            return obj;
        }

        throw new InvalidCastException();
    }
}

[Guid(AnyTypeIdentifiers.AnyTypeT3GuidString)]
public readonly struct AnyType<T1, T2, T3> : IEquatable<AnyType<T1, T2, T3>>
{
    public AnyType(T1 val)
    {
        Value = val;
    }

    public AnyType(T2 val)
    {
        Value = val;
    }

    public AnyType(T3 val)
    {
        Value = val;
    }

    public object? Value { get; }

    public readonly T1 First => (T1)this;

    public readonly T2 Second => (T2)this;

    public readonly T3 Third => (T3)this;

    public readonly bool TryGetFirst([MaybeNullWhen(false)] out T1 value)
    {
        if (Value is T1 obj)
        {
            value = obj;
            return true;
        }

        value = default;
        return false;
    }

    public readonly bool TryGetSecond([MaybeNullWhen(false)] out T2 value)
    {
        if (Value is T2 obj)
        {
            value = obj;
            return true;
        }

        value = default;
        return false;
    }

    public readonly bool TryGetThird([MaybeNullWhen(false)] out T3 value)
    {
        if (Value is T3 obj)
        {
            value = obj;
            return true;
        }

        value = default;
        return false;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is AnyType<T1, T2, T3> other && Equals(other);
    }

    public readonly bool Equals(AnyType<T1, T2, T3> other)
    {
        return EqualityComparer<object>.Default.Equals(Value, other.Value);
    }

    public override readonly int GetHashCode()
    {
        if (Value is null)
        {
            return 0;
        }

        return EqualityComparer<object>.Default.GetHashCode(Value);
    }

    public static bool operator ==(AnyType<T1, T2, T3> left, AnyType<T1, T2, T3> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AnyType<T1, T2, T3> left, AnyType<T1, T2, T3> right)
    {
        return !(left == right);
    }

    public static implicit operator AnyType<T1, T2, T3>(T1 val)
    {
        return new(val);
    }

    public static implicit operator AnyType<T1, T2, T3>?(T1? val)
    {
        return val != null ? new AnyType<T1, T2, T3>?(new AnyType<T1, T2, T3>(val)) : new AnyType<T1, T2, T3>?();
    }

    public static implicit operator AnyType<T1, T2, T3>(T2 val)
    {
        return new(val);
    }

    public static implicit operator AnyType<T1, T2, T3>?(T2? val)
    {
        return val != null ? new AnyType<T1, T2, T3>?(new AnyType<T1, T2, T3>(val)) : new AnyType<T1, T2, T3>?();
    }

    public static implicit operator AnyType<T1, T2, T3>(T3 val)
    {
        return new(val);
    }

    public static implicit operator AnyType<T1, T2, T3>?(T3? val)
    {
        return val != null ? new AnyType<T1, T2, T3>?(new AnyType<T1, T2, T3>(val)) : new AnyType<T1, T2, T3>?();
    }

    public static explicit operator T1(AnyType<T1, T2, T3> sum)
    {
        if (sum.Value is T1 obj)
        {
            return obj;
        }

        throw new InvalidCastException();
    }

    public static explicit operator T2(AnyType<T1, T2, T3> sum)
    {
        if (sum.Value is T2 obj)
        {
            return obj;
        }

        throw new InvalidCastException();
    }

    public static explicit operator T3(AnyType<T1, T2, T3> sum)
    {
        if (sum.Value is T3 obj)
        {
            return obj;
        }

        throw new InvalidCastException();
    }
}

[Guid(AnyTypeIdentifiers.AnyTypeT4GuidString)]
public readonly struct AnyType<T1, T2, T3, T4> : IEquatable<AnyType<T1, T2, T3, T4>>
{
    public AnyType(T1 val)
    {
        Value = val;
    }

    public AnyType(T2 val)
    {
        Value = val;
    }

    public AnyType(T3 val)
    {
        Value = val;
    }

    public AnyType(T4 val)
    {
        Value = val;
    }

    public object? Value { get; }

    public readonly T1 First => (T1)this;

    public readonly T2 Second => (T2)this;

    public readonly T3 Third => (T3)this;

    public readonly T4 Fourth => (T4)this;

    public readonly bool TryGetFirst([MaybeNullWhen(false)] out T1 value)
    {
        if (Value is T1 obj)
        {
            value = obj;
            return true;
        }

        value = default;
        return false;
    }

    public readonly bool TryGetSecond([MaybeNullWhen(false)] out T2 value)
    {
        if (Value is T2 obj)
        {
            value = obj;
            return true;
        }

        value = default;
        return false;
    }

    public readonly bool TryGetThird([MaybeNullWhen(false)] out T3 value)
    {
        if (Value is T3 obj)
        {
            value = obj;
            return true;
        }

        value = default;
        return false;
    }

    public readonly bool TryGetFourth([MaybeNullWhen(false)] out T4 value)
    {
        if (Value is T4 obj)
        {
            value = obj;
            return true;
        }

        value = default;
        return false;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is AnyType<T1, T2, T3, T4> other && Equals(other);
    }

    public readonly bool Equals(AnyType<T1, T2, T3, T4> other)
    {
        return EqualityComparer<object>.Default.Equals(Value, other.Value);
    }

    public override readonly int GetHashCode()
    {
        if (Value is null)
        {
            return 0;
        }

        return EqualityComparer<object>.Default.GetHashCode(Value);
    }

    public static bool operator ==(AnyType<T1, T2, T3, T4> left, AnyType<T1, T2, T3, T4> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AnyType<T1, T2, T3, T4> left, AnyType<T1, T2, T3, T4> right)
    {
        return !(left == right);
    }

    public static implicit operator AnyType<T1, T2, T3, T4>(T1 val)
    {
        return new(val);
    }

    public static implicit operator AnyType<T1, T2, T3, T4>?(T1? val)
    {
        return val != null ? new AnyType<T1, T2, T3, T4>?(new AnyType<T1, T2, T3, T4>(val)) : new AnyType<T1, T2, T3, T4>?();
    }

    public static implicit operator AnyType<T1, T2, T3, T4>(T2 val)
    {
        return new(val);
    }

    public static implicit operator AnyType<T1, T2, T3, T4>?(T2? val)
    {
        return val != null ? new AnyType<T1, T2, T3, T4>?(new AnyType<T1, T2, T3, T4>(val)) : new AnyType<T1, T2, T3, T4>?();
    }

    public static implicit operator AnyType<T1, T2, T3, T4>(T3 val)
    {
        return new(val);
    }

    public static implicit operator AnyType<T1, T2, T3, T4>?(T3? val)
    {
        return val != null ? new AnyType<T1, T2, T3, T4>?(new AnyType<T1, T2, T3, T4>(val)) : new AnyType<T1, T2, T3, T4>?();
    }

    public static implicit operator AnyType<T1, T2, T3, T4>(T4 val)
    {
        return new(val);
    }

    public static implicit operator AnyType<T1, T2, T3, T4>?(T4? val)
    {
        return val != null ? new AnyType<T1, T2, T3, T4>?(new AnyType<T1, T2, T3, T4>(val)) : new AnyType<T1, T2, T3, T4>?();
    }

    public static explicit operator T1(AnyType<T1, T2, T3, T4> sum)
    {
        if (sum.Value is T1 obj)
        {
            return obj;
        }

        throw new InvalidCastException();
    }

    public static explicit operator T2(AnyType<T1, T2, T3, T4> sum)
    {
        if (sum.Value is T2 obj)
        {
            return obj;
        }

        throw new InvalidCastException();
    }

    public static explicit operator T3(AnyType<T1, T2, T3, T4> sum)
    {
        if (sum.Value is T3 obj)
        {
            return obj;
        }

        throw new InvalidCastException();
    }

    public static explicit operator T4(AnyType<T1, T2, T3, T4> sum)
    {
        if (sum.Value is T4 obj)
        {
            return obj;
        }

        throw new InvalidCastException();
    }
}
