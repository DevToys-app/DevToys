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
        private Color _textColor;
        private Color _backgroundColor;
        private bool _wcagAaLevelLargeText;
        private bool _wcagAaLevelSmallText;
        private bool _wcagAaaLevelLargeText;
        private bool _wcagAaaLevelSmallText;

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
        public Color TextColor
        {
            get => _textColor;
            set
            {
                SetProperty(ref _textColor, value);
                CalculateContrastRatio();
            }
        }

        /// <summary>
        /// Gets or sets the selected color.
        /// </summary>
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                SetProperty(ref _backgroundColor, value);
                CalculateContrastRatio();
            }
        }

        /// <summary>
        /// Gets whether the color contrast pass the WCAG AA level for large text.
        /// </summary>
        public bool WcagAaLevelLargeText
        {
            get => _wcagAaLevelLargeText;
            private set => SetProperty(ref _wcagAaLevelLargeText, value);
        }

        /// <summary>
        /// Gets whether the color contrast pass the WCAG AA level for small text.
        /// </summary>
        public bool WcagAaLevelSmallText
        {
            get => _wcagAaLevelSmallText;
            private set => SetProperty(ref _wcagAaLevelSmallText, value);
        }

        /// <summary>
        /// Gets whether the color contrast pass the WCAG AAA level for large text.
        /// </summary>
        public bool WcagAaaLevelLargeText
        {
            get => _wcagAaaLevelLargeText;
            private set => SetProperty(ref _wcagAaaLevelLargeText, value);
        }

        /// <summary>
        /// Gets whether the color contrast pass the WCAG AAA level for small text.
        /// </summary>
        public bool WcagAaaLevelSmallText
        {
            get => _wcagAaaLevelSmallText;
            private set => SetProperty(ref _wcagAaaLevelSmallText, value);
        }

        [ImportingConstructor]
        public ColorPickerToolViewModel(ISettingsProvider settingsProvider, IThemeListener themeListener)
        {
            _settingsProvider = settingsProvider;

            string? accentColorResourceName = themeListener.ActualAppTheme == ApplicationTheme.Light ? "SystemAccentColorLight2" : "SystemAccentColorDark1";
            BackgroundColor = (Color)Application.Current.Resources[accentColorResourceName];
            TextColor = themeListener.ActualAppTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
        }

        private void CalculateContrastRatio()
        {
            double textLuminence = CalculateLuminence(TextColor);
            double backgroundLuminence = CalculateLuminence(BackgroundColor);

            double contrastRatio
                = textLuminence > backgroundLuminence
                ? ((backgroundLuminence + 0.05) / (textLuminence + 0.05))
                : ((textLuminence + 0.05) / (backgroundLuminence + 0.05));

            WcagAaLevelLargeText = contrastRatio < 1 / 3f;
            WcagAaLevelSmallText = contrastRatio < 1 / 4.5;
            WcagAaaLevelLargeText = contrastRatio < 1 / 4.5;
            WcagAaaLevelSmallText = contrastRatio < 1 / 7f;
        }

        private double CalculateLuminence(Color color)
        {
            double r = Luminence(color.R);
            double g = Luminence(color.G);
            double b = Luminence(color.B);

            return r * 0.2126 + g * 0.7152 + b * 0.0722;

            static double Luminence(double value)
            {
                value /= 255;
                return value < 0.03928
                    ? value / 12.92
                    : Math.Pow((value + 0.055) / 1.055, 2.4);
            }
        }
    }
}
