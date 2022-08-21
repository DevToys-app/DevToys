#nullable enable

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

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public JwtDecoderControlViewModel ViewModel
        {
            get => (JwtDecoderControlViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        [ImportingConstructor]
        public JwtDecoderControl()
        {
            InitializeComponent();
        }
    }
}
