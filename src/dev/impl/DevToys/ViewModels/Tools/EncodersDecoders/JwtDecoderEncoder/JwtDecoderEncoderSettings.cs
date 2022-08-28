#nullable enable

using DevToys.Api.Core.Settings;
using DevToys.Models;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    internal static class JwtDecoderEncoderSettings
    {
        /// <summary>
        /// Define if we need to show the validation parameters / inputs
        /// </summary>
        public static readonly SettingDefinition<bool> ShowValidation
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ShowValidation)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token signature
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateSignature
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ValidateSignature)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token issuer
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateIssuer
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ValidateIssuer)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token actor
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateActor
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ValidateActor)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token audience
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateAudience
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ValidateAudience)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token lifetime
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateLifetime
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ValidateLifetime)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token lifetime
        /// </summary>
        public static readonly SettingDefinition<bool> JWtToolMode
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(JWtToolMode)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token has an expiration
        /// </summary>
        public static readonly SettingDefinition<bool> HasExpiration
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(HasExpiration)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token has a default time
        /// </summary>
        public static readonly SettingDefinition<bool> HasDefaultTime
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(HasDefaultTime)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token has a default time
        /// </summary>
        public static readonly SettingDefinition<bool> HasAudience
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(HasAudience)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token has a default time
        /// </summary>
        public static readonly SettingDefinition<string?> ValidAudiences
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ValidAudiences)}",
                isRoaming: true,
                defaultValue: string.Empty);

        /// <summary>
        /// Define if the token has a default time
        /// </summary>
        public static readonly SettingDefinition<bool> HasIssuer
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(HasIssuer)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token has a default time
        /// </summary>
        public static readonly SettingDefinition<string?> ValidIssuers
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ValidIssuers)}",
                isRoaming: true,
                defaultValue: string.Empty);

        /// <summary>
        /// Define if the token expiration year
        /// </summary>
        public static readonly SettingDefinition<int> ExpireYear
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ExpireYear)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration month
        /// </summary>
        public static readonly SettingDefinition<int> ExpireMonth
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ExpireMonth)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration day
        /// </summary>
        public static readonly SettingDefinition<int> ExpireDay
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ExpireDay)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration hours
        /// </summary>
        public static readonly SettingDefinition<int> ExpireHour
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ExpireHour)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration minutes
        /// </summary>
        public static readonly SettingDefinition<int> ExpireMinute
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(ExpireMinute)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration minutes
        /// </summary>
        public static readonly SettingDefinition<JwtAlgorithm> JwtAlgorithm
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(JwtAlgorithm)}",
                isRoaming: true,
                defaultValue: Models.JwtAlgorithm.HS256);

        /// <summary>
        /// Define if the token expiration minutes
        /// </summary>
        public static readonly SettingDefinition<string?> PublicKey
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(PublicKey)}",
                isRoaming: true,
                defaultValue: string.Empty);

        /// <summary>
        /// Define if the token expiration minutes
        /// </summary>
        public static readonly SettingDefinition<string?> PrivateKey
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(PrivateKey)}",
                isRoaming: true,
                defaultValue: string.Empty);

        /// <summary>
        /// Define if the token expiration minutes
        /// </summary>
        public static readonly SettingDefinition<string?> Signature
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModelBase)}.{nameof(Signature)}",
                isRoaming: true,
                defaultValue: string.Empty);
    }
}
