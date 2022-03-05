#nullable enable 

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.ViewModels.Tools.Converters.NumberBaseConverter;
using DevToys.ViewModels.Tools.NumberBaseConverter;
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
    public sealed partial class BasicNumberBaseConverterControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(BasicNumberBaseConverterControlViewModel),
                typeof(BasicNumberBaseConverterControl),
                new PropertyMetadata(new BasicNumberBaseConverterControlViewModel()));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public BasicNumberBaseConverterControlViewModel ViewModel
        {
            get => (BasicNumberBaseConverterControlViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public BasicNumberBaseConverterControl()
        {
            this.InitializeComponent();
        }
    }
}
