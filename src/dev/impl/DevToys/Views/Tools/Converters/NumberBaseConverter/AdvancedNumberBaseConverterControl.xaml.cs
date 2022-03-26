#nullable enable
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using DevToys.ViewModels.Tools.Converters.NumberBaseConverter;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace DevToys.Views.Tools.Converters.NumberBaseConverter
{
    public sealed partial class AdvancedNumberBaseConverterControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(AdvancedNumberBaseConverterControlViewModel),
            typeof(AdvancedNumberBaseConverterControl),
            new PropertyMetadata(default(AdvancedNumberBaseConverterControlViewModel)));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public AdvancedNumberBaseConverterControlViewModel ViewModel
        {
            get => (AdvancedNumberBaseConverterControlViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        [ImportingConstructor]
        public AdvancedNumberBaseConverterControl()
        {
            InitializeComponent();
        }
    }
}
