using System.Reflection;
using System.Runtime.Serialization;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Core;

/// <summary>
/// Provides a set of extension for enumerations
/// </summary>
internal static class EnumExtension
{
    /// <summary>
    /// Retrieves the <see cref="DescriptionAttribute"/>'s value.
    /// </summary>
    /// <typeparam name="T">The targeted enumeration</typeparam>
    /// <param name="enumerationValue">The value</param>
    /// <returns>A string that corresponds to the description of the enumeration.</returns>
    internal static string GetDescription<T>(this T enumerationValue) where T : struct, Enum
    {
        Guard.IsNotNull(enumerationValue!);

        MemberInfo[]? memberInfo = enumerationValue.GetType().GetMember(enumerationValue.ToString()!);

        if (memberInfo is not null && memberInfo.Length > 0)
        {
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).ToArray();

            if (attrs is not null && attrs.Length > 0)
                return ((DescriptionAttribute)attrs[0]).Description;
        }

        return enumerationValue.ToString()!;
    }

    /// <summary>
    /// Converts the given text into an enum.
    /// </summary>
    internal static T ToEnum<T>(this string input) where T : struct, Enum
    {
        Type enumType = typeof(T);

        if (Enum.TryParse(input, out T result))
            return result;

        foreach (string name in Enum.GetNames(enumType))
        {
            var fieldInfo = enumType.GetField(name)?.GetCustomAttributes(typeof(EnumMemberAttribute), true) as EnumMemberAttribute[];
            if (fieldInfo is not null && fieldInfo.Length == 1)
            {
                if (fieldInfo[0].Value == input)
                    return (T)Enum.Parse(enumType, name);
            }
        }

        return default;
    }
}
