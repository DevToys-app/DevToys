#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Helpers.JsonYaml;
using DevToys.Models;
using DevToys.Models.JwtDecoderEncoder;
using DevToys.UI.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    public abstract class JwtDecoderEncoderViewModelBase : ObservableRecipient
    {
        private string? _token;
        private string? _header;
        private string? _payload;
        private bool _requireSignature;
        private JwtAlgorithmDisplayPair? _algorithmSelected;
        private InfoBarData? _validationResult;

        protected bool WorkInProgress;
        protected bool ToolSuccessfullyWorked;
        protected readonly Queue<bool> JobQueue = new();
        protected readonly ISettingsProvider SettingsProvider;
        protected readonly IMarketingService MarketingService;

        internal ValidationBase JwtValidation = new();

        internal JwtDecoderEncoderStrings LocalizedStrings => LanguageManager.Instance.JwtDecoderEncoder;

        internal RoutedEventHandler InputFocusChanged { get; }

        internal virtual string? Token
        {
            get => _token;
            set
            {
                if (_token != value)
                {
                    SetProperty(ref _token, value?.Trim());
                    QueueNewTokenJob();
                }
            }
        }

        internal string? Header
        {
            get => _header;
            set
            {
                if (_header != value)
                {
                    SetProperty(ref _header, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal string? Payload
        {
            get => _payload;
            set
            {
                if (_payload != value)
                {
                    SetProperty(ref _payload, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool ValidateSignature
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateSignature);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateSignature, value);
                OnPropertyChanged();
                ShowValidation = value;
                QueueNewTokenJob();
            }
        }

        internal bool ValidateIssuer
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateIssuer);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateIssuer, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal bool ValidateActor
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateActor);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateActor, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal bool ValidateAudience
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateAudience);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateAudience, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal bool ValidateLifetime
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidateLifetime);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidateLifetime, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal string? ValidIssuers
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidIssuers);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidIssuers, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal string? ValidAudiences
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ValidAudiences);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ValidAudiences, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal string? PublicKey
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.PublicKey);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.PublicKey, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal string? PrivateKey
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.PrivateKey);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.PrivateKey, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal string? Signature
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.Signature);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.Signature, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal bool ShowValidation
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ShowValidation);
            set
            {
                SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ShowValidation, value);
                OnPropertyChanged();
                QueueNewTokenJob();
            }
        }

        internal bool RequireSignature
        {
            get => _requireSignature;
            set
            {
                if (_requireSignature != value)
                {
                    SetProperty(ref _requireSignature, value);
                    OnPropertyChanged();
                }
            }
        }

        internal InfoBarData? ValidationResult
        {
            get => _validationResult;
            private set => SetProperty(ref _validationResult, value);
        }

        internal JwtAlgorithmDisplayPair AlgorithmMode
        {
            get
            {
                JwtAlgorithm settingsValue = SettingsProvider.GetSetting(JwtDecoderEncoderSettings.JwtAlgorithm);
                JwtAlgorithmDisplayPair? algorithm = Algorithms.FirstOrDefault(x => x.Value == settingsValue);
                Header = JsonHelper.Format(@"{""alg"": """ + algorithm.DisplayName + @""", ""typ"": ""JWT""}", Indentation.TwoSpaces, false);
                IsSignatureRequired(algorithm);
                return _algorithmSelected ?? JwtAlgorithmDisplayPair.HS256;
            }
            set
            {
                if (_algorithmSelected != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.JwtAlgorithm, value.Value);
                    IsSignatureRequired(value);
                    SetProperty(ref _algorithmSelected, value);
                    OnPropertyChanged();
                }
            }
        }

        internal IReadOnlyList<JwtAlgorithmDisplayPair> Algorithms = new ObservableCollection<JwtAlgorithmDisplayPair> {
            JwtAlgorithmDisplayPair.HS256, JwtAlgorithmDisplayPair.HS384, JwtAlgorithmDisplayPair.HS512,
            JwtAlgorithmDisplayPair.RS256, JwtAlgorithmDisplayPair.RS384, JwtAlgorithmDisplayPair.RS512,
            JwtAlgorithmDisplayPair.ES256, JwtAlgorithmDisplayPair.ES384, JwtAlgorithmDisplayPair.ES512,
            JwtAlgorithmDisplayPair.PS256, JwtAlgorithmDisplayPair.PS384, JwtAlgorithmDisplayPair.PS512,
        };

        public JwtDecoderEncoderViewModelBase(
            ISettingsProvider settingsProvider,
            IMarketingService marketingService)
        {
            SettingsProvider = settingsProvider;
            MarketingService = marketingService;
            InputFocusChanged = ControlFocusChanged;
            IsSignatureRequired(AlgorithmMode);
        }

        internal void QueueNewTokenJob()
        {
            JwtValidation = new ValidationBase
            {
                IsValid = true
            };
            var newJob = new JwtJobAddedMessage();
            Messenger.Send(newJob);
        }

        protected void DisplayValidationInfoBar()
        {
            InfoBarSeverity infoBarSeverity;
            string message;
            if (!JwtValidation.IsValid)
            {
                infoBarSeverity = InfoBarSeverity.Error;
                message = JwtValidation.ErrorMessage ?? LocalizedStrings.JwtInValidMessage;
            }
            else
            {
                infoBarSeverity = InfoBarSeverity.Success;
                message = LocalizedStrings.JwtIsValidMessage;
            }

            ValidationResult = new InfoBarData(infoBarSeverity, message);
        }

        protected void ControlFocusChanged(object source, RoutedEventArgs args)
        {
            var input = (CustomTextBox)source;

            if (input.Text.Length == 0)
            {
                return;
            }

            QueueNewTokenJob();
        }

        private void IsSignatureRequired(JwtAlgorithmDisplayPair value)
        {
            if (value.Value is JwtAlgorithm.HS256 ||
                value.Value is JwtAlgorithm.HS384 ||
                value.Value is JwtAlgorithm.HS512)
            {
                RequireSignature = true;
            }
            else
            {
                RequireSignature = false;
            }
        }

        protected void TokenErrorCallBack(TokenResultErrorEventArgs e)
        {
            JwtValidation.IsValid = false;
            JwtValidation.ErrorMessage = e.Message;
        }
    }
}
