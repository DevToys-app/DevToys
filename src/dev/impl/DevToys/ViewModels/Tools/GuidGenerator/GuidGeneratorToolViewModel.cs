#nullable enable

using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.UI.Controls;
using DevToys.Views.Tools.GuidGenerator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Composition;
using System.Text;

namespace DevToys.ViewModels.Tools.GuidGenerator
{
    [Export(typeof(GuidGeneratorToolViewModel))]
    public sealed class GuidGeneratorToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Whether the generated GUID should be uppercase or lowercase.
        /// </summary>
        private static readonly SettingDefinition<bool> Uppercase
            = new(
                name: $"{nameof(GuidGeneratorToolViewModel)}.{nameof(Uppercase)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Whether the generated GUID should have hyphens or not.
        /// </summary>
        private static readonly SettingDefinition<bool> Hyphens
            = new(
                name: $"{nameof(GuidGeneratorToolViewModel)}.{nameof(Hyphens)}",
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// How many GUIDs should be generated at once.
        /// </summary>
        private static readonly SettingDefinition<int> GuidsToGenerate
            = new(
                name: $"{nameof(GuidGeneratorToolViewModel)}.{nameof(GuidsToGenerate)}",
                isRoaming: true,
                defaultValue: 1);

        private readonly ISettingsProvider _settingsProvider;
        private string _output = string.Empty;

        public Type View { get; } = typeof(GuidGeneratoToolPage);

        internal GuidGeneratorStrings Strings => LanguageManager.Instance.GuidGenerator;

        internal bool IsUppercase
        {
            get => _settingsProvider.GetSetting(Uppercase);
            set
            {
                if (_settingsProvider.GetSetting(Uppercase) != value)
                {
                    _settingsProvider.SetSetting(Uppercase, value);
                    OnPropertyChanged();
                }
            }
        }

        internal bool IncludeHyphens
        {
            get => _settingsProvider.GetSetting(Hyphens);
            set
            {
                if (_settingsProvider.GetSetting(Hyphens) != value)
                {
                    _settingsProvider.SetSetting(Hyphens, value);
                    OnPropertyChanged();
                }
            }
        }

        internal int NumberOfGuidsToGenerate
        {
            get => _settingsProvider.GetSetting(GuidsToGenerate);
            set
            {
                if (_settingsProvider.GetSetting(GuidsToGenerate) != value)
                {
                    _settingsProvider.SetSetting(GuidsToGenerate, value);
                    OnPropertyChanged();
                }
            }
        }

        internal string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        internal ICustomTextBox? OutputTextBox { private get; set; }

        [ImportingConstructor]
        public GuidGeneratorToolViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;

            GenerateCommand = new RelayCommand(ExecuteGenerateCommand);
        }

        #region GenerateCommand

        internal IRelayCommand GenerateCommand { get; }

        private void ExecuteGenerateCommand()
        {
            string guidStringFormat;
            if (IncludeHyphens)
            {
                guidStringFormat = "D";
            }
            else
            {
                guidStringFormat = "N";
            }

            var newGuids = new StringBuilder();
            for (int i = 0; i < NumberOfGuidsToGenerate; i++)
            {
                string guid = Guid.NewGuid().ToString(guidStringFormat);
                if (IsUppercase)
                {
                    guid = guid.ToUpperInvariant();
                }

                newGuids.AppendLine(guid);
            }

            Output += newGuids.ToString();
            OutputTextBox?.ScrollToBottom();
        }

        #endregion
    }
}
