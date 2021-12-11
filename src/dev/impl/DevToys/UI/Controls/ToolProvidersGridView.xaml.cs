#nullable enable

using System.Collections.Generic;
using DevToys.Api.Tools;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevToys.UI.Controls
{
    public sealed partial class ToolProvidersGridView : UserControl
    {
        public static readonly DependencyProperty ToolsProperty
            = DependencyProperty.Register(
                nameof(Tools),
                typeof(IEnumerable<IToolProvider>),
                typeof(ToolProvidersGridView),
                new PropertyMetadata(null));

        public IEnumerable<IToolProvider>? Tools
        {
            get => (IEnumerable<IToolProvider>?)GetValue(ToolsProperty);
            set => SetValue(ToolsProperty, value);
        }

        public static readonly DependencyProperty ToolClickCommandProperty
            = DependencyProperty.Register(
                nameof(ToolClickCommand),
                typeof(IRelayCommand<IToolProvider>),
                typeof(ToolProvidersGridView),
                new PropertyMetadata(null));

        public IRelayCommand<IToolProvider>? ToolClickCommand
        {
            get => (IRelayCommand<IToolProvider>?)GetValue(ToolClickCommandProperty);
            set => SetValue(ToolClickCommandProperty, value);
        }

        public static readonly DependencyProperty OpenToolInNewWindowClickCommandProperty
            = DependencyProperty.Register(
                nameof(OpenToolInNewWindowClickCommand),
                typeof(IRelayCommand<IToolProvider>),
                typeof(ToolProvidersGridView),
                new PropertyMetadata(null));

        public IRelayCommand<IToolProvider>? OpenToolInNewWindowClickCommand
        {
            get => (IRelayCommand<IToolProvider>?)GetValue(OpenToolInNewWindowClickCommandProperty);
            set => SetValue(OpenToolInNewWindowClickCommandProperty, value);
        }

        public ToolProvidersGridView()
        {
            InitializeComponent();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ToolClickCommand?.Execute((IToolProvider)e.ClickedItem);
        }

        private void OpenInNewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            OpenToolInNewWindowClickCommand?.Execute(((FrameworkElement)sender).DataContext);
        }
    }
}
