#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.EncodersDecoders.JwtDecoderEncoder;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    [Export(typeof(JwtEncoderControlViewModel))]
    public sealed class JwtEncoderControlViewModel : JwtDecoderEncoderViewModel, IToolViewModel, IRecipient<JwtJobAddedMessage>
    {
        private string? _token;
        private bool _hasExpiration;
        private bool _hasDefaultTime;
        private bool _hasAudience;
        private bool _hasIssuer;
        private bool _hasError;
        private int _expireYear = 0;
        private int _expireMonth = 0;
        private int _expireDay = 0;
        private int _expireHour = 0;
        private int _expireMinute = 0;

        internal override string? Token
        {
            get => _token;
            set
            {
                if (_token != value)
                {
                    SetProperty(ref _token, value?.Trim());
                }
            }
        }

        internal bool HasExpiration
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.HasExpiration);
            set
            {
                if (_hasExpiration != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.HasExpiration, value);
                    SetProperty(ref _hasExpiration, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool HasAudience
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.HasAudience);
            set
            {
                if (_hasAudience != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.HasAudience, value);
                    SetProperty(ref _hasAudience, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool HasIssuer
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.HasIssuer);
            set
            {
                if (_hasIssuer != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.HasIssuer, value);
                    SetProperty(ref _hasIssuer, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool HasDefaultTime
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.HasDefaultTime);
            set
            {
                if (_hasDefaultTime != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.HasDefaultTime, value);
                    SetProperty(ref _hasDefaultTime, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool HasError
        {
            get => _hasError;
            set
            {
                SetProperty(ref _hasError, value);
            }
        }

        internal int ExpireYear
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ExpireYear);
            set
            {
                if (value < 1)
                {
                    return;
                }
                if (_expireYear != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireYear, value);
                    SetProperty(ref _expireYear, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal int ExpireMonth
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ExpireMonth);
            set
            {
                if (value < 1)
                {
                    return;
                }
                if (_expireMonth != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireMonth, value);
                    SetProperty(ref _expireMonth, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal int ExpireDay
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ExpireDay);
            set
            {
                if (value < 1)
                {
                    return;
                }
                if (_expireDay != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireDay, value);
                    SetProperty(ref _expireDay, value);
                    QueueNewTokenJob();
                }
            }

        }

        internal int ExpireHour
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ExpireHour);
            set
            {
                if (value < 1)
                {
                    return;
                }
                if (_expireHour != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireHour, value);
                    SetProperty(ref _expireHour, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal int ExpireMinute
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ExpireMinute);
            set
            {
                if (value < 1)
                {
                    return;
                }
                if (_expireMinute != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireMinute, value);
                    SetProperty(ref _expireMinute, value);
                    QueueNewTokenJob();
                }
            }
        }

        public Type View { get; } = typeof(JwtEncoderControl);

        [ImportingConstructor]
        public JwtEncoderControlViewModel(
           ISettingsProvider settingsProvider,
           IMarketingService marketingService)
           : base(settingsProvider, marketingService)
        {
            IsActive = true;
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
                GenerateToken(out string? tokenString, AlgorithmMode.Value);
                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    Token = tokenString;

                    if (!ToolSuccessfullyWorked)
                    {
                        ToolSuccessfullyWorked = true;
                        MarketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            WorkInProgress = false;
        }

        private void GenerateToken(out string tokenString, JwtAlgorithm algorithmMode)
        {
            if (string.IsNullOrWhiteSpace(Header) || string.IsNullOrWhiteSpace(Payload))
            {
                tokenString = string.Empty;
                return;
            }

            try
            {
                var serializeOptions = new JsonSerializerOptions();
                serializeOptions.Converters.Add(new JwtPayloadConverter());
                Dictionary<string, object>? payload = JsonSerializer.Deserialize<Dictionary<string, object>>(Payload!, serializeOptions);
                SigningCredentials? signingCredentials = GetSigningCredentials(algorithmMode);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Claims = payload,
                    SigningCredentials = signingCredentials,
                    Expires = null
                };

                if (HasExpiration)
                {
                    DateTime expirationDate = DateTime.UtcNow
                        .AddYears(ExpireYear)
                        .AddMonths(ExpireMonth)
                        .AddDays(ExpireDay)
                        .AddHours(ExpireHour)
                        .AddMinutes(ExpireMinute);
                    tokenDescriptor.Expires = expirationDate;
                }

                if (HasAudience)
                {
                    tokenDescriptor.Audience = ValidAudiences;
                }

                if (HasIssuer)
                {
                    tokenDescriptor.Issuer = ValidIssuers;
                    tokenDescriptor.IssuedAt = DateTime.UtcNow;
                }

                var handler = new JwtSecurityTokenHandler();
                if (!HasDefaultTime)
                {
                    handler.SetDefaultTimesOnTokenCreation = false;
                }

                SecurityToken? token = handler.CreateToken(tokenDescriptor);
                tokenString = handler.WriteToken(token);
            }
            catch (Exception exception)
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = exception.Message;
                tokenString = string.Empty;
            }
        }
    }
}
