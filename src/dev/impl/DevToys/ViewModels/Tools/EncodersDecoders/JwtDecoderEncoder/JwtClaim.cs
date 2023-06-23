#nullable enable

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Windows.ApplicationModel.Resources;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    public class JwtClaim
    {
        private readonly Claim _claim;
        
        public JwtClaim(Claim claim) => _claim = claim;

        public string Type => _claim.Type;
        
        public string Value => DateFields.Contains(Type) && long.TryParse(_claim.Value, out long value)
            ? DateTimeOffset.FromUnixTimeSeconds(value).ToLocalTime().ToString()
            : _claim.Value;

        public string? Description =>
            TryGetDescription(_claim.Type.ToLowerInvariant(), out string? description)
                ? description
                : LanguageManager.Instance.JwtDecoderEncoder.NoDescription;

        #region Statics

        private static readonly ResourceLoader Resources = ResourceLoader.GetForViewIndependentUse(nameof(JwtDecoderEncoder));

        private static bool TryGetDescription(string claim, out string? description)
        {
            string descriptionResource = Resources.GetString(claim);
            if (string.IsNullOrEmpty(descriptionResource))
            {
                description = null;
                return false;
            }
            else
            {
                description = descriptionResource;
                return true;
            }
        }

        private static readonly List<string> DateFields = new() { "exp", "nbf", "iat", "auth_time", "updated_at" };

        #endregion
    }
}
