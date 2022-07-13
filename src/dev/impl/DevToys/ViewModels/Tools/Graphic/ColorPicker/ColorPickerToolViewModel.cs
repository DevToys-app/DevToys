#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using DevToys.Api.Core.Settings;
using DevToys.Api.Core.Theme;
using DevToys.Api.Tools;
using DevToys.Models;
using DevToys.Views.Tools.ColorPicker;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml;

namespace DevToys.ViewModels.Tools.Graphic.ColorPicker
{
    [Export(typeof(ColorPickerToolViewModel))]
    public sealed class ColorPickerToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// The mode of the color picker.
        /// </summary>
        private static readonly SettingDefinition<ColorSpectrumComponents> Mode
            = new(
                name: $"{nameof(ColorPickerToolViewModel)}.{nameof(Mode)}",
                isRoaming: true,
                defaultValue: ColorPickerModeDisplayPair.HSV.Value);

        private readonly ISettingsProvider _settingsProvider;
        private Color _selectedColor;

        public Type View => typeof(ColorPickerToolPage);

        internal ColorPickerStrings Strings => LanguageManager.Instance.ColorPicker;

        /// <summary>
        /// Gets or sets the desired color picker mode.
        /// </summary>
        public ColorPickerModeDisplayPair ColorPickerMode
        {
            get
            {
                ColorSpectrumComponents settingsValue = _settingsProvider.GetSetting(Mode);
                ColorPickerModeDisplayPair? mode = ColorPickerModes.FirstOrDefault(x => x.Value == settingsValue);
                return mode ?? ColorPickerModeDisplayPair.HSV;
            }
            set
            {
                if (ColorPickerMode != value)
                {
                    _settingsProvider.SetSetting(Mode, value.Value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get a list of supported color picker modes
        /// </summary>
        public IReadOnlyList<ColorPickerModeDisplayPair> ColorPickerModes
            = new ObservableCollection<ColorPickerModeDisplayPair>
            {
                ColorPickerModeDisplayPair.HSV,
                ColorPickerModeDisplayPair.HSL
            };

        /// <summary>
        /// Gets or sets the selected color.
        /// </summary>
        public Color SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }

        [ImportingConstructor]
        public ColorPickerToolViewModel(ISettingsProvider settingsProvider, IThemeListener themeListener)
        {
            _settingsProvider = settingsProvider;

            string? accentColorResourceName = themeListener.ActualAppTheme == ApplicationTheme.Light ? "SystemAccentColorLight2" : "SystemAccentColorDark1";
            SelectedColor = (Color)Application.Current.Resources[accentColorResourceName];
        }
    }
}
