#nullable enable

using System;
using System.Composition;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder;
using DevToys.Views.Tools.JwtDecoderEncoder;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.JwtDecoderEncoder
{
    [Export(typeof(JwtDecoderEncoderToolViewModel))]
    public sealed class JwtDecoderEncoderToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly ISettingsProvider _settingsProvider;

        internal bool JwtToolMode
        {
            get => _settingsProvider.GetSetting(JwtDecoderEncoderSettings.JWtToolMode);
            set
            {
                _settingsProvider.SetSetting(JwtDecoderEncoderSettings.JWtToolMode, value);
                OnPropertyChanged();
            }
        }

        internal JwtDecoderEncoderStrings Strings => LanguageManager.Instance.JwtDecoderEncoder;

        public Type View { get; } = typeof(JwtDecoderEncoderToolPage);

        public JwtDecoderControlViewModel DecoderViewModel { get; private set; }

        public JwtEncoderControlViewModel EncoderViewModel { get; private set; }

        [ImportingConstructor]
        public JwtDecoderEncoderToolViewModel(
            JwtDecoderControlViewModel decoderControlViewModel,
            JwtEncoderControlViewModel encoderControlViewModel,
            ISettingsProvider settingsProvider)
        {
            DecoderViewModel = decoderControlViewModel;
            EncoderViewModel = encoderControlViewModel;
            _settingsProvider = settingsProvider;
            IsActive = true;
        }
    }
}
