#nullable enable

using System;
using System.Composition;
using DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevToys.Views.Tools.EncodersDecoders.JwtDecoderEncoder
{
    public sealed partial class JwtDecoderControl : UserControl
    {
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
            InitializeComponent();
        }

        private void PayloadCodeEditor_ExpandedChanged(object sender, System.EventArgs e)
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
    }
}
