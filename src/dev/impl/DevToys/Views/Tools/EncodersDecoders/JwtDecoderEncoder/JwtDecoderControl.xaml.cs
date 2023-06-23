#nullable enable

using System;
using System.Composition;
using DevToys.Api.Core.Settings;
using DevToys.Core.Settings;
using DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder;
using Microsoft.Toolkit.Uwp.UI.Controls.Primitives;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DevToys.Views.Tools.EncodersDecoders.JwtDecoderEncoder
{
    public sealed partial class JwtDecoderControl : UserControl
    {
        private readonly ISettingsProvider? _settingsProvider;

        public static readonly DependencyProperty ViewModelProperty
           = DependencyProperty.Register(
               nameof(ViewModel),
               typeof(JwtDecoderControlViewModel),
               typeof(JwtDecoderControl),
               new PropertyMetadata(default(JwtDecoderControlViewModel)));


        public event EventHandler? ExpandedChanged;

        public bool IsExpanded { get; private set; }

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public JwtDecoderControlViewModel ViewModel
        {
            get => (JwtDecoderControlViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public JwtDecoderControl()
        {
            _settingsProvider = Shared.Core.MefComposer.Provider.Import<ISettingsProvider>();
            _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;

            InitializeComponent();
        }

        private void SettingsProvider_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            ApplySettings();
        }

        private void PayloadCodeEditor_ExpandedChanged(object sender, EventArgs e)
        {
            IsExpanded = !IsExpanded;

            if (PayloadCodeEditor.IsExpanded)
            {
                JwtDecoderGrid.Children.Remove(PayloadCodeEditor);
                ExpandedChanged?.Invoke(PayloadCodeEditor, EventArgs.Empty);
            }
            else
            {
                ExpandedChanged?.Invoke(PayloadCodeEditor, EventArgs.Empty);
                JwtDecoderGrid.Children.Add(PayloadCodeEditor);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            if (_settingsProvider is not null && InfoDataGrid is not null)
            {
                FontFamily fontFamily = new(_settingsProvider.GetSetting(PredefinedSettings.TextEditorFont));
                InfoDataGrid.FontFamily = fontFamily;
                Style style = new(typeof(DataGridColumnHeader));
                style.Setters.Add(new Setter(FontFamilyProperty, fontFamily));
                InfoDataGrid.ColumnHeaderStyle = style;
            }
        }
    }
}
