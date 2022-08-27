#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Models.JwtDecoderEncoder;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.EncodersDecoders.JwtDecoderEncoder;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    [Export(typeof(JwtEncoderControlViewModel))]
    public sealed class JwtEncoderControlViewModel : JwtDecoderEncoderViewModel, IToolViewModel, IRecipient<JwtJobAddedMessage>
    {
        private string? _token;
        private bool _hasError;

        private readonly JwtEncoder _encoder;

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
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.HasExpiration, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal bool HasAudience
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.HasAudience);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.HasAudience, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal bool HasIssuer
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.HasIssuer);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.HasIssuer, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal bool HasDefaultTime
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.HasDefaultTime);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.HasDefaultTime, value);
                OnPropertyChanged();
                QueueNewTokenJob();
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
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireYear, value);
                OnPropertyChanged();
                QueueNewTokenJob();
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
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireMonth, value);
                OnPropertyChanged();
                QueueNewTokenJob();
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
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireDay, value);
                OnPropertyChanged();
                QueueNewTokenJob();
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
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireHour, value);
                OnPropertyChanged();
                QueueNewTokenJob();
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
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ExpireMinute, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        public Type View { get; } = typeof(JwtEncoderControl);

        [ImportingConstructor]
        public JwtEncoderControlViewModel(
           ISettingsProvider settingsProvider,
           IMarketingService marketingService,
           JwtEncoder encoder)
           : base(settingsProvider, marketingService)
        {
            IsActive = true;
            _encoder = encoder;
        }

        public void Receive(JwtJobAddedMessage message)
        {
            if (string.IsNullOrWhiteSpace(Payload))
            {
                return;
            }

            EncoderParameters encoderParameters = new()
            {
                HasAudience = HasAudience,
                HasExpiration = HasExpiration,
                HasIssuer = HasIssuer,
                HasDefaultTime = HasDefaultTime
            };

            TokenParameters tokenParameters = new()
            {
                TokenAlgorithm = AlgorithmMode.Value,
                Payload = Payload,
                ExpirationYear = ExpireYear,
                ExpirationMonth = ExpireMonth,
                ExpirationDay = ExpireDay,
                ExpirationHour = ExpireHour,
                ExpirationMinute = ExpireMinute
            };

            if (!string.IsNullOrEmpty(ValidIssuers))
            {
                tokenParameters.ValidIssuers = ValidIssuers!.Split(',').ToHashSet();
            }

            if (!string.IsNullOrEmpty(ValidAudiences))
            {
                tokenParameters.ValidAudiences = ValidAudiences!.Split(',').ToHashSet();
            }

            if (AlgorithmMode.Value is JwtAlgorithm.HS256 ||
                AlgorithmMode.Value is JwtAlgorithm.HS384 ||
                AlgorithmMode.Value is JwtAlgorithm.HS512)
            {
                tokenParameters.Signature = Signature;
            }
            else
            {
                tokenParameters.PrivateKey = PrivateKey;
            }

            TokenResult? result = _encoder.GenerateToken(encoderParameters, tokenParameters, TokenErrorCallBack);

            ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
            {
                if (result is not null)
                {
                    Token = result.Token;
                }
                HasError = JwtValidation.IsValid!;
                if (ToolSuccessfullyWorked)
                {
                    ToolSuccessfullyWorked = true;
                    MarketingService.NotifyToolSuccessfullyWorked();
                }
            }).ForgetSafely();
        }
    }
}
