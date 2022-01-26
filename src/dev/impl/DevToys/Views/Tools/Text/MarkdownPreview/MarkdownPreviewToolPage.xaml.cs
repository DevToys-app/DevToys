#nullable enable

using System;
using DevToys.Api.Core.Navigation;
using DevToys.Core;
using DevToys.Messages;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.MarkdownPreview;
using Microsoft.Toolkit.Mvvm.Messaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace DevToys.Views.Tools.MarkdownPreview
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MarkdownPreviewToolPage : Page, IRecipient<NavigateToMarkdownPreviewHtmlMessage>
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(MarkdownPreviewToolViewModel),
                typeof(MarkdownPreviewToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public MarkdownPreviewToolViewModel ViewModel
        {
            get => (MarkdownPreviewToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public MarkdownPreviewToolPage()
        {
            InitializeComponent();

            // Register all recipient for messages
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            if (ViewModel is null)
            {
                // Set the view model
                Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
                ViewModel = (MarkdownPreviewToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            if (!string.IsNullOrWhiteSpace(parameters.ClipBoardContent))
            {
                ViewModel.InputValue = parameters.ClipBoardContent;
            }

            base.OnNavigatedTo(e);
        }

        public void Receive(NavigateToMarkdownPreviewHtmlMessage message)
        {
            Arguments.NotNull(message, nameof(message));
            PreviewWebView.NavigateToString(message.Html);
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                string markdownBodyEl = "document.getElementsByClassName('markdown-body')[0].innerHTML";
                data.SetText(await PreviewWebView.InvokeScriptAsync("eval", new string[] { markdownBodyEl }) ?? string.Empty);

                Clipboard.SetContentWithOptions(data, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
                Clipboard.Flush();
            }
            catch (Exception ex)
            {
                Logger.LogFault("Failed to copy from webview", ex);
            }
        }
    }
}
