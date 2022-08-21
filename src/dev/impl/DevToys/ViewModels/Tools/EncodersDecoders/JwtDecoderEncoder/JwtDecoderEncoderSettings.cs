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
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ShowValidation)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token signature
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateSignature
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ValidateSignature)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token issuer
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateIssuer
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ValidateIssuer)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token actor
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateActor
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ValidateActor)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token audience
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateAudience
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ValidateAudience)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token lifetime
        /// </summary>
        public static readonly SettingDefinition<bool> ValidateLifetime
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ValidateLifetime)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if we need to validate the token lifetime
        /// </summary>
        public static readonly SettingDefinition<bool> JWtToolMode
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(JWtToolMode)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token has an expiration
        /// </summary>
        public static readonly SettingDefinition<bool> HasExpiration
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(HasExpiration)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token has a default time
        /// </summary>
        public static readonly SettingDefinition<bool> HasDefaultTime
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(HasDefaultTime)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token has a default time
        /// </summary>
        public static readonly SettingDefinition<bool> HasAudience
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(HasAudience)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token has a default time
        /// </summary>
        public static readonly SettingDefinition<bool> HasIssuer
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(HasIssuer)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Define if the token expiration year
        /// </summary>
        public static readonly SettingDefinition<int> ExpireYear
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ExpireYear)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration month
        /// </summary>
        public static readonly SettingDefinition<int> ExpireMonth
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ExpireMonth)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration day
        /// </summary>
        public static readonly SettingDefinition<int> ExpireDay
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ExpireDay)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration hours
        /// </summary>
        public static readonly SettingDefinition<int> ExpireHour
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ExpireHour)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration minutes
        /// </summary>
        public static readonly SettingDefinition<int> ExpireMinute
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(ExpireMinute)}",
                isRoaming: true,
                defaultValue: 0);

        /// <summary>
        /// Define if the token expiration minutes
        /// </summary>
        public static readonly SettingDefinition<JwtAlgorithm> JwtAlgorithm
            = new(
                name: $"{nameof(JwtDecoderEncoderViewModel)}.{nameof(JwtAlgorithm)}",
                isRoaming: true,
                defaultValue: Models.JwtAlgorithm.HS256);
    }
}
