#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Messages;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.UI.Controls;
using DevToys.ViewModels.Tools.Converters.NumberBaseConverter;
using DevToys.Views.Tools.NumberBaseConverter;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Windows.UI.Xaml;

namespace DevToys.ViewModels.Tools.NumberBaseConverter
{
    [Export(typeof(NumberBaseConverterToolViewModel))]
    public sealed class NumberBaseConverterToolViewModel : ObservableRecipient, IToolViewModel, IRecipient<ChangeInfoBarStatusMessage>
    {
        /// <summary>
        /// Whether the value should be formatted or not.
        /// </summary>
        private static readonly SettingDefinition<bool> Formatted
            = new(
                name: $"{nameof(NumberBaseConverterToolViewModel)}.{nameof(Formatted)}",
                isRoaming: true,
                defaultValue: true);

        private readonly Queue<string> _convertQueue = new();
        private readonly ISettingsProvider _settingsProvider;

        private string? _infoBarMessage;
        private bool _isInfoBarOpen;
        private bool _advancedMode;

        internal NumberBaseConverterStrings Strings => LanguageManager.Instance.NumberBaseConverter;

        public Type View { get; } = typeof(NumberBaseConverterToolPage);
        public AdvancedNumberBaseConverterControlViewModel AdvancedViewModel { get; private set; }
        public BasicNumberBaseConverterControlViewModel BasicViewModel { get; private set; }

        internal bool AdvancedMode
        {
            get => _advancedMode;
            set
            {
                SetProperty(ref _advancedMode, value);
            }
        }

        internal bool IsInfoBarOpen
        {
            get => _isInfoBarOpen;
            set => SetProperty(ref _isInfoBarOpen, value);
        }

        internal string? InfoBarMessage
        {
            get => _infoBarMessage;
            set => SetProperty(ref _infoBarMessage, value);
        }

        internal bool IsFormatted
        {
            get => _settingsProvider.GetSetting(Formatted);
            set
            {
                if (_settingsProvider.GetSetting(Formatted) != value)
                {
                    _settingsProvider.SetSetting(Formatted, value);
                    OnPropertyChanged();
                    Messenger.Send(new ChangeNumberFormattingMessage(value));
                }
            }
        }

        [ImportingConstructor]
        public NumberBaseConverterToolViewModel(ISettingsProvider settingsProvider, AdvancedNumberBaseConverterControlViewModel advnacedViewModel, 
            BasicNumberBaseConverterControlViewModel basicViewModel)
        {
            _settingsProvider = settingsProvider;
            AdvancedViewModel = advnacedViewModel;
            BasicViewModel = basicViewModel;
            IsActive = true;
        }        

        public void Receive(ChangeInfoBarStatusMessage message)
        {
            InfoBarMessage = message.Message;
            IsInfoBarOpen = message.IsOpen;
        }
    }
}
