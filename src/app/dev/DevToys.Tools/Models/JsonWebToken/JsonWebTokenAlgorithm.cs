namespace DevToys.Tools.Models;

internal enum JsonWebTokenAlgorithm
{
    HS256 = 0,
    HS384 = 1,
    HS512 = 2,

    RS256 = 3,
    RS384 = 4,
    RS512 = 5,

    ES256 = 6,
    ES384 = 7,
    ES512 = 8,

    PS256 = 9,
    PS384 = 10,
    PS512 = 11
}
