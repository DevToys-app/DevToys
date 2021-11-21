#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Messages;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.MarkdownPreview;
using Microsoft.Toolkit.Mvvm.Messaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
    }
}
