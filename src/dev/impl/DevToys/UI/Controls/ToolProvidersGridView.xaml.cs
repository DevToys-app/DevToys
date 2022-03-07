#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevToys.Api.Tools;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevToys.UI.Controls
{
    public sealed partial class ToolProvidersGridView : UserControl
    {
        public static readonly DependencyProperty ToolsProperty
            = DependencyProperty.Register(
                nameof(Tools),
                typeof(IEnumerable<ToolProviderViewItem>),
                typeof(ToolProvidersGridView),
                new PropertyMetadata(null));

        public IEnumerable<ToolProviderViewItem>? Tools
        {
            get => (IEnumerable<ToolProviderViewItem>?)GetValue(ToolsProperty);
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
                typeof(IRelayCommand<ToolProviderMetadata>),
                typeof(ToolProvidersGridView),
                new PropertyMetadata(null));

        public IRelayCommand<ToolProviderMetadata>? OpenToolInNewWindowClickCommand
        {
            get => (IRelayCommand<ToolProviderMetadata>?)GetValue(OpenToolInNewWindowClickCommandProperty);
            set => SetValue(OpenToolInNewWindowClickCommandProperty, value);
        }

        public static readonly DependencyProperty PinToolToStartClickCommandProperty
            = DependencyProperty.Register(
                nameof(PinToolToStartClickCommand),
                typeof(IRelayCommand<ToolProviderMetadata>),
                typeof(ToolProvidersGridView),
                new PropertyMetadata(null));

        public IRelayCommand<ToolProviderMetadata>? PinToolToStartClickCommand
        {
            get => (IRelayCommand<ToolProviderMetadata>?)GetValue(PinToolToStartClickCommandProperty);
            set => SetValue(PinToolToStartClickCommandProperty, value);
        }

        public static readonly DependencyProperty AddToFavoritesCommandProperty
            = DependencyProperty.Register(
                nameof(AddToFavoritesCommand),
                typeof(IRelayCommand<ToolProviderViewItem>),
                typeof(ToolProvidersGridView),
                new PropertyMetadata(null));

        public IRelayCommand<ToolProviderViewItem>? AddToFavoritesCommand
        {
            get => (IRelayCommand<ToolProviderViewItem>?)GetValue(AddToFavoritesCommandProperty);
            set => SetValue(AddToFavoritesCommandProperty, value);
        }

        public static readonly DependencyProperty RemoveFromFavoritesCommandProperty
            = DependencyProperty.Register(
                nameof(RemoveFromFavoritesCommand),
                typeof(IRelayCommand<ToolProviderViewItem>),
                typeof(ToolProvidersGridView),
                new PropertyMetadata(null));

        public IRelayCommand<ToolProviderViewItem>? RemoveFromFavoritesCommand
        {
            get => (IRelayCommand<ToolProviderViewItem>?)GetValue(RemoveFromFavoritesCommandProperty);
            set => SetValue(RemoveFromFavoritesCommandProperty, value);
        }

        public ToolProvidersGridView()
        {
            InitializeComponent();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tool = (ToolProviderViewItem)e.ClickedItem;
            ToolClickCommand?.Execute(tool.ToolProvider);
        }

        private void OpenInNewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var tool = (ToolProviderViewItem)((FrameworkElement)e.OriginalSource).DataContext;
            OpenToolInNewWindowClickCommand?.Execute(tool.Metadata);
        }

        private void PinToolToStartCommandButton_Click(object sender, RoutedEventArgs e)
        {
            var tool = (ToolProviderViewItem)((FrameworkElement)e.OriginalSource).DataContext;
            PinToolToStartClickCommand?.Execute(tool.Metadata);
        }

        private void AddToFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            var tool = (ToolProviderViewItem)((FrameworkElement)e.OriginalSource).DataContext;
            AddToFavoritesCommand?.Execute(tool);
        }

        private void RemoveFromFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            var tool = (ToolProviderViewItem)((FrameworkElement)e.OriginalSource).DataContext;
            RemoveFromFavoritesCommand?.Execute(tool);
        }
    }
}
