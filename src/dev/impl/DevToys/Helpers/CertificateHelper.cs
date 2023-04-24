#nullable enable

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DevToys.Helpers
{
    internal static class CertificateHelper
    {
        private const string BeginCertificate = "BEGIN CERTIFICATE";
        private const string BeginCertificateRequest = "BEGIN CERTIFICATE REQUEST";
        private const int IncorrectPassword = -2147024810;

        /// <summary>
        /// Returns the friendly formatted, decoded certificate details.
        /// CSR format is currently not supported in UWP, but is in .NET 7 <see href="https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.certificaterequest.loadsigningrequest?view=net-7.0"/>
        /// PEM format is limited by the current framework version <see href="https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2.createfrompemfile?view=net-7.0"/>
        /// </summary>
        /// <param name="input">Certificate data</param>
        /// <param name="password">Password for protected private key.  This should only be relevant for decoding pfx format.</param>
        /// <returns>Certificate details</returns>
        internal static bool TryDecodeCertificate(string input, string? password, out string? decoded)
        {
            decoded = null;
            string publicCert = input;
            if (publicCert.Contains('-'))
            {
                string[] splitCert = input.Split('-', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < splitCert.Length; i++)
                {
                    if (string.Equals(splitCert[i], BeginCertificate, StringComparison.OrdinalIgnoreCase))
                    {
                        publicCert = splitCert[i + 1];
                        break;
                    }
                    // If this is a valid certificate request, we should still return true with the error message
                    else if (string.Equals(splitCert[i], BeginCertificateRequest, StringComparison.OrdinalIgnoreCase))
                    {
                        decoded = LanguageManager.Instance.CertificateEncoderDecoder.UnsupportedFormatError;
                        return true;
                    }
                }
            }

            try
            {
                var certificate = new X509Certificate2(Convert.FromBase64String(publicCert), password);
                decoded = certificate.ToString();
            }
            catch (CryptographicException wce)
            {
                // If this is a valid certificate, but an incorrect/missing password, we should still return true
                if (wce.HResult == IncorrectPassword)
                {
                    decoded = LanguageManager.Instance.CertificateEncoderDecoder.InvalidPasswordError;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the string data from a certificate file.
        /// If the string is pem format with plain text values, it will return as is.
        /// Otherwise, the base64 encoded result will be returned.
        /// </summary>
        /// <param name="data">Certificate data from file</param>
        /// <returns>string result of certificate</returns>
        internal static string GetRawCertificateString(byte[] data)
        {
            string value = Encoding.UTF8.GetString(data);
            if (value.Contains(BeginCertificate, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            return Convert.ToBase64String(data);
        }
    }
}
