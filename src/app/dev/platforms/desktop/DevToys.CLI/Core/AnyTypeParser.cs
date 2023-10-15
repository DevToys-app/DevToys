using System.Diagnostics.CodeAnalysis;

namespace DevToys.CLI.Core;

internal static class AnyTypeParser
{
    private delegate bool TryParseString(string token, [NotNullWhen(true)] out object? value);

    // Forked from https://github.com/dotnet/command-line-api/blob/d12a6ff555b33670defb2243daecdc247caf88e8/src/System.CommandLine/Binding/ArgumentConverter.StringConverters.cs#L15-L289
    private static readonly Dictionary<Type, TryParseString> _stringParsers
        = new()
        {
            [typeof(bool)] = (string token, [NotNullWhen(true)] out object? value) =>
            {
                if (bool.TryParse(token, out bool parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(DateOnly)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                if (DateOnly.TryParse(input, out DateOnly parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(DateTime)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                if (DateTime.TryParse(input, out DateTime parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(DateTimeOffset)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                if (DateTimeOffset.TryParse(input, out DateTimeOffset parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(decimal)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                if (decimal.TryParse(input, out decimal parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(DirectoryInfo)] = (string path, [NotNullWhen(true)] out object? value) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    value = default;
                    return false;
                }
                value = new DirectoryInfo(path);
                return true;
            },

            [typeof(double)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                if (double.TryParse(input, out double parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(FileInfo)] = (string path, [NotNullWhen(true)] out object? value) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    value = default;
                    return false;
                }
                value = new FileInfo(path);
                return true;
            },

            [typeof(FileSystemInfo)] = (string path, [NotNullWhen(true)] out object? value) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    value = default;
                    return false;
                }
                if (Directory.Exists(path))
                {
                    value = new DirectoryInfo(path);
                }
                else if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                         path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    value = new DirectoryInfo(path);
                }
                else
                {
                    value = new FileInfo(path);
                }

                return true;
            },

            [typeof(float)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                if (float.TryParse(input, out float parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(Guid)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                if (Guid.TryParse(input, out Guid parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(int)] = (string token, [NotNullWhen(true)] out object? value) =>
            {
                if (int.TryParse(token, out int intValue))
                {
                    value = intValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(long)] = (string token, [NotNullWhen(true)] out object? value) =>
            {
                if (long.TryParse(token, out long longValue))
                {
                    value = longValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(short)] = (string token, [NotNullWhen(true)] out object? value) =>
            {
                if (short.TryParse(token, out short shortValue))
                {
                    value = shortValue;
                    return true;
                }

                value = default;
                return false;
            },

#if NET6_0_OR_GREATER
            [typeof(TimeOnly)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                if (TimeOnly.TryParse(input, out TimeOnly parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },
#endif

            [typeof(uint)] = (string token, [NotNullWhen(true)] out object? value) =>
            {
                if (uint.TryParse(token, out uint uintValue))
                {
                    value = uintValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(sbyte)] = (string token, [NotNullWhen(true)] out object? value) =>
            {
                if (sbyte.TryParse(token, out sbyte sbyteValue))
                {
                    value = sbyteValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(byte)] = (string token, [NotNullWhen(true)] out object? value) =>
            {
                if (byte.TryParse(token, out byte byteValue))
                {
                    value = byteValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(string)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                value = input;
                return true;
            },

            [typeof(ulong)] = (string token, [NotNullWhen(true)] out object? value) =>
            {
                if (ulong.TryParse(token, out ulong ulongValue))
                {
                    value = ulongValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(ushort)] = (string token, [NotNullWhen(true)] out object? value) =>
            {
                if (ushort.TryParse(token, out ushort ushortValue))
                {
                    value = ushortValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(TimeSpan)] = (string input, [NotNullWhen(true)] out object? value) =>
            {
                if (TimeSpan.TryParse(input, out TimeSpan timeSpan))
                {
                    value = timeSpan;
                    return true;
                }

                value = default;
                return false;
            },
        };

    internal static object? ParseAnyType(AnyTypeOption anyTypeOption, string token)
    {
        // anyTypeOption.AnyTypeDefinition is either of type
        // - AnyType<T1, T2>
        // - AnyType<T1, T2, T3>
        // - AnyType<T1, T2, T3, T4>
        // For each generic type argument, we will try to parse the token as that type.

        var parsedArguments = new List<object?>();
        for (int j = 0; j < anyTypeOption.AnyTypeDefinition.GenericTypeArguments.Length; j++)
        {
            Type genericTypeArgument = anyTypeOption.AnyTypeDefinition.GenericTypeArguments[j];

            try
            {
                object? parsedArgument = ParseScalarValue(token, genericTypeArgument);
                if (parsedArgument is not null)
                {
                    parsedArguments.Add(parsedArgument);
                }
            }
            catch
            {
            }
        }

        return SelectBestMatch(parsedArguments, anyTypeOption.AnyTypeDefinition);
    }

    private static object? ParseScalarValue(string value, Type type)
    {
        // If the type is nullable, we will try to parse the value as the underlying type.
        if (type.TryGetNullableType(out Type? nullableType))
        {
            return ParseScalarValue(value, nullableType);
        }

        if (_stringParsers.TryGetValue(type, out TryParseString? tryConvert))
        {
            if (tryConvert(value, out object? converted))
            {
                return converted;
            }
            else
            {
                return null;
            }
        }

        if (type.IsEnum)
        {
            if (Enum.TryParse(type, value, ignoreCase: true, out object? converted))
            {
                return converted;
            }
        }

        return null;
    }

    private static object? SelectBestMatch(List<object?> parsedArguments, Type anyTypeDefinition)
    {
        if (parsedArguments.Count <= 0)
        {
            // We could not parse the token as any of the generic type arguments.
            return null;
        }
        else if (parsedArguments.Count == 1)
        {
            // We could parse the token as one of the generic type arguments.
            return Activator.CreateInstance(anyTypeDefinition, parsedArguments[0]);
        }
        else
        {
            // We could parse the token as more than one of the generic type arguments.
            // Let's return the first one in the order of priority of parsed values based on the order of keys of _stringParsers.
            // This way, when we have something like AnyType<string, float>, we will return the float value when the input is something
            // like "100" which match a float as well as a string.
            foreach (Type parserType in _stringParsers.Keys)
            {
                foreach (object? argument in parsedArguments)
                {
                    if (argument is null)
                    {
                        continue;
                    }

                    if (parserType.IsAssignableFrom(argument.GetType()))
                    {
                        return Activator.CreateInstance(anyTypeDefinition, argument);
                    }
                }
            }

            return null;
        }
    }
}
