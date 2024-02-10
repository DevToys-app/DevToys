using DevToys.Tools.Models;
using YamlDotNet.Core.Tokens;

namespace DevToys.Tools.Tools.EncodersDecoders.Jwt;

internal enum JsonWebTokenGridRows
{
    Settings,
    SubContainer
}

internal enum GridColumns
{
    Stretch
}

internal enum JsonWebTokenExpirationGridRow
{
    Content
}

internal enum JsonWebTokenExpirationGridColumn
{
    Year,
    Month,
    Day,
    Hour,
    Minute
}
