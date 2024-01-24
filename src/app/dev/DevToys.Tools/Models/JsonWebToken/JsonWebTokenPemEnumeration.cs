using System.Text;

namespace DevToys.Tools.Models.JwtDecoderEncoder;

internal class JsonWebTokenPemEnumeration
{
    public static readonly JsonWebTokenPemEnumeration PublicKey = new("-----BEGIN PUBLIC KEY-----", "-----END PUBLIC KEY-----");

    public static readonly JsonWebTokenPemEnumeration PrivateKey = new("-----BEGIN PRIVATE KEY-----", "-----END PRIVATE KEY-----");

    public static readonly JsonWebTokenPemEnumeration RsaPublicKey = new("-----BEGIN RSA PUBLIC KEY-----", "-----END RSA PUBLIC KEY-----");

    public static readonly JsonWebTokenPemEnumeration RsaPrivateKey = new("-----BEGIN RSA PRIVATE KEY-----", "-----END RSA PRIVATE KEY-----");

    public static readonly JsonWebTokenPemEnumeration RsaEncryptedPrivateKey = new("-----BEGIN ENCRYPTED PRIVATE KEY-----", "-----END ENCRYPTED PRIVATE KEY-----");

    public static readonly JsonWebTokenPemEnumeration ECDPublicKey = new("-----BEGIN EC PUBLIC KEY-----", "-----END EC PUBLIC KEY-----");

    public static readonly JsonWebTokenPemEnumeration ECDPrivateKey = new("-----BEGIN EC PRIVATE KEY-----", "-----END EC PRIVATE KEY-----");

    public string PemStart { get; }

    public string PemEnd { get; }

    public static byte[] GetBytes(JsonWebTokenPemEnumeration jwtPem, string key)
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

    private JsonWebTokenPemEnumeration(string pemStart, string pemEnd)
    {
        Guard.IsNotNullOrWhiteSpace(pemStart);
        Guard.IsNotNullOrWhiteSpace(pemEnd);
        PemStart = pemStart;
        PemEnd = pemEnd;
    }
}
