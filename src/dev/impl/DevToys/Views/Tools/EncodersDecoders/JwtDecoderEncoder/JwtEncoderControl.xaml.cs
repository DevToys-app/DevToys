#nullable enable

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

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public JwtEncoderControlViewModel ViewModel
        {
            get => (JwtEncoderControlViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        [ImportingConstructor]
        public JwtEncoderControl()
        {
            InitializeComponent();
        }
    }
}
