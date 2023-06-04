namespace DevToys.Blazor.Core.Extensions;

internal static class EnumExtensions
{
    internal static string ToDescriptionString(this Enum value)
    {
        System.Reflection.FieldInfo? field = value.GetType().GetField(value.ToString());
        if (field is null)
        {
            return value.ToString().ToLower();
        }

        var attributes = Attribute.GetCustomAttributes(field, typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        return attributes is { Length: > 0 }
            ? attributes[0].Description
            : value.ToString().ToLower();
    }
}
