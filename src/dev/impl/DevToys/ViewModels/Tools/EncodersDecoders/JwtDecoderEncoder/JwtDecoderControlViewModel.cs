#nullable enable

using System;
using System.Composition;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Helpers.JsonYaml;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.EncodersDecoders.JwtDecoderEncoder;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    [Export(typeof(JwtDecoderControlViewModel))]
    public sealed class JwtDecoderControlViewModel : JwtDecoderEncoderViewModel, IToolViewModel, IRecipient<JwtJobAddedMessage>
    {
        private bool _validateSignature;
        private bool _validateIssuer;
        private bool _validateActor;
        private bool _validateAudience;
        private bool _validateLifetime;

        internal bool ValidateSignature
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateSignature);
            set
            {
                if (_validateSignature != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateSignature, value);
                    SetProperty(ref _validateSignature, value);
                    ShowValidation = value;
                    QueueNewTokenJob();
                }
            }
        }

        internal bool ValidateIssuer
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateIssuer);
            set
            {
                if (_validateIssuer != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateIssuer, value);
                    SetProperty(ref _validateIssuer, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool ValidateActor
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateActor);
            set
            {
                if (_validateActor != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateActor, value);
                    SetProperty(ref _validateActor, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool ValidateAudience
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateAudience);
            set
            {
                if (_validateAudience != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateAudience, value);
                    SetProperty(ref _validateAudience, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool ValidateLifetime
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateLifetime);
            set
            {
                if (_validateLifetime != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateLifetime, value);
                    SetProperty(ref _validateLifetime, value);
                    QueueNewTokenJob();
                }
            }
        }

        public Type View { get; } = typeof(JwtDecoderControl);

        [ImportingConstructor]
        public JwtDecoderControlViewModel(
           ISettingsProvider settingsProvider,
           IMarketingService marketingService)
           : base(settingsProvider, marketingService)
        {
            IsActive = true;
            ShowValidation = ValidateSignature;
        }

        public void Receive(JwtJobAddedMessage message)
        {
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (WorkInProgress)
            {
                return;
            }

            WorkInProgress = true;

            await TaskScheduler.Default;

            while (JobQueue.TryDequeue(out _))
            {
                DecodeToken(out string? header, out string? payload, out object? tokenAlgorithm);
                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    Header = header;
                    Payload = payload;

                    if (tokenAlgorithm is not null)
                    {
                        RequireSignature = true;
                        if ((JwtAlgorithm)tokenAlgorithm is
                            not Models.JwtAlgorithm.HS256 and
                            not Models.JwtAlgorithm.HS384 and
                            not Models.JwtAlgorithm.HS512)
                        {
                            RequireSignature = false;
                        }
                    }

                    if (ValidateSignature && !string.IsNullOrWhiteSpace(Token))
                    {
                        DisplayValidationInfoBar();
                    }

                    if (!ToolSuccessfullyWorked)
                    {
                        ToolSuccessfullyWorked = true;
                        MarketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            WorkInProgress = false;
        }

        private void DecodeToken(out string header, out string payload, out object? tokenAlgorithm)
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                header = string.Empty;
                payload = string.Empty;
                tokenAlgorithm = null;
                return;
            }

            try
            {
                IdentityModelEventSource.ShowPII = true;
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(Token);
                header = JsonHelper.Format(jwtSecurityToken.Header.SerializeToJson(), Indentation.TwoSpaces, false);
                payload = JsonHelper.Format(jwtSecurityToken.Payload.SerializeToJson(), Indentation.TwoSpaces, false);
                Enum.TryParse(typeof(JwtAlgorithm), jwtSecurityToken.SignatureAlgorithm, out tokenAlgorithm);

                if (ValidateSignature)
                {
                    SigningCredentials? signingCredentials = GetValidationCredentials((JwtAlgorithm)tokenAlgorithm);

                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingCredentials.Key,
                        TryAllIssuerSigningKeys = true,
                        ValidateActor = ValidateActor,
                        ValidateLifetime = ValidateLifetime,
                        ValidateIssuer = ValidateIssuer,
                        ValidateAudience = ValidateAudience
                    };

                    if (ValidateIssuer)
                    {
                        if (string.IsNullOrWhiteSpace(ValidIssuers))
                        {
                            JwtValidation.IsValid = false;
                            JwtValidation.ErrorMessage = LocalizedStrings.ValidIssuersError;
                            return;
                        }
                        validationParameters.ValidIssuers = ValidIssuers!.Split(',');
                    }

                    if (ValidateAudience)
                    {
                        if (string.IsNullOrWhiteSpace(ValidAudiences))
                        {
                            JwtValidation.IsValid = false;
                            JwtValidation.ErrorMessage = LocalizedStrings.ValidAudiencesError;
                            return;
                        }
                        validationParameters.ValidAudiences = ValidAudiences!.Split(',');
                    }

                    try
                    {
                        handler.ValidateToken(Token, validationParameters, out _);
                        JwtValidation.IsValid = true;
                    }
                    catch (Exception exception)
                    {
                        JwtValidation.IsValid = false;
                        JwtValidation.ErrorMessage = exception.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                header = ex.Message;
                payload = ex.Message;
                tokenAlgorithm = null;
            }
        }
    }
}
