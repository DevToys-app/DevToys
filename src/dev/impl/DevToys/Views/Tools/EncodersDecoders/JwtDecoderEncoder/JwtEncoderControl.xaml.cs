#nullable enable

using System;
using System.Composition;
using DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevToys.Views.Tools.EncodersDecoders.JwtDecoderEncoder
{
    public sealed partial class JwtEncoderControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(JwtEncoderControlViewModel),
                typeof(JwtEncoderControl),
                new PropertyMetadata(default(JwtEncoderControlViewModel)));

        public event EventHandler? ExpandedChanged;

        public bool IsExpanded { get; private set; }

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public JwtEncoderControlViewModel ViewModel
        {
            get => (JwtEncoderControlViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public JwtEncoderControl()
        {
            InitializeComponent();
        }

        private void PayloadCodeEditor_ExpandedChanged(object sender, System.EventArgs e)
        {
            IsExpanded = !IsExpanded;
            
            if (PayloadCodeEditor.IsExpanded)
            {
                JwtEncoderGrid.Children.Remove(PayloadCodeEditor);
                ExpandedChanged?.Invoke(PayloadCodeEditor, EventArgs.Empty);
            }
            else
            {
                ExpandedChanged?.Invoke(PayloadCodeEditor, EventArgs.Empty);
                JwtEncoderGrid.Children.Add(PayloadCodeEditor);
            }
        }
    }
}
