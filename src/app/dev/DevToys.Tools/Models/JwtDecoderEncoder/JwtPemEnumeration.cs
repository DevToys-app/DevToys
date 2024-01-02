using System.Text;

namespace DevToys.Tools.Models.JwtDecoderEncoder;

internal class JwtPemEnumeration
{
    public static readonly JwtPemEnumeration PublicKey = new("-----BEGIN PUBLIC KEY-----", "-----END PUBLIC KEY-----");

    public static readonly JwtPemEnumeration RsaPublicKey = new("-----BEGIN RSA PUBLIC KEY-----", "-----END RSA PUBLIC KEY-----");

    public static readonly JwtPemEnumeration ECDPublicKey = new("-----BEGIN EC PUBLIC KEY-----", "-----END EC PUBLIC KEY-----");

    public string PemStart { get; }

    public string PemEnd { get; }

    public static byte[] GetBytes(JwtPemEnumeration jwtPem, string key)
    {
        var keyStringBuilder = new StringBuilder(key!.Trim());
        keyStringBuilder.Replace(Environment.NewLine, string.Empty);
        if (key.StartsWith(jwtPem.PemStart, StringComparison.OrdinalIgnoreCase))
        {
            keyStringBuilder.Remove(0, jwtPem.PemStart.Length);
        }
        if (key.Contains(jwtPem.PemEnd, StringComparison.OrdinalIgnoreCase))
        {
            keyStringBuilder.Replace(jwtPem.PemEnd, string.Empty);
        }
        return Convert.FromBase64String(keyStringBuilder.ToString());
    }

    private JwtPemEnumeration(string pemStart, string pemEnd)
    {
        Guard.IsNotNullOrWhiteSpace(pemStart);
        Guard.IsNotNullOrWhiteSpace(pemEnd);
        PemStart = pemStart;
        PemEnd = pemEnd;
    }
}
