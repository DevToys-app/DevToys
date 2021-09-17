#nullable enable

using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevTools.Common.UI.Controls
{
    public sealed partial class CustomTextBox : UserControl
    {
        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(CustomTextBox),
                new PropertyMetadata(null));

        public object Header
        {
            get => (object)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty
            = DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(CustomTextBox),
                new PropertyMetadata(false, OnIsReadOnlyPropertyChangedCalled));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static readonly DependencyProperty AcceptsReturnProperty
            = DependencyProperty.Register(
                nameof(AcceptsReturn),
                typeof(bool),
                typeof(CustomTextBox),
                new PropertyMetadata(false, OnAcceptsReturnPropertyChangedCalled));

        public bool AcceptsReturn
        {
            get => (bool)GetValue(AcceptsReturnProperty);
            set => SetValue(AcceptsReturnProperty, value);
        }

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(CustomTextBox),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public CustomTextBox()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void UpdateCopyButtons()
        {
            if (IsReadOnly)
            {
                if (!AcceptsReturn)
                {
                    GetInlinedCopyButton().Visibility = Visibility.Visible;
                }
                else
                {
                    GetCopyButton().Visibility = Visibility.Visible;
                }

                if (PasteButton is not null)
                {
                    GetPasteButton().Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (AcceptsReturn)
                {
                    GetPasteButton().Visibility = Visibility.Visible;
                }

                if (InlinedCopyButton is not null)
                {
                    GetInlinedCopyButton().Visibility = Visibility.Collapsed;
                }

                if (CopyButton is not null)
                {
                    GetCopyButton().Visibility = Visibility.Collapsed;
                }
            }
        }

        private Button GetCopyButton()
        {
            return (Button)(CopyButton ?? FindName(nameof(CopyButton)));
        }

        private Button GetPasteButton()
        {
            return (Button)(PasteButton ?? FindName(nameof(PasteButton)));
        }

        private Button GetInlinedCopyButton()
        {
            return (Button)(InlinedCopyButton ?? FindName(nameof(InlinedCopyButton)));
        }

        private ContentPresenter GetHeaderContentPresenter()
        {
            return (ContentPresenter)(HeaderContentPresenter ?? FindName(nameof(HeaderContentPresenter)));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Header is not null)
            {
                GetHeaderContentPresenter().Visibility = Visibility.Visible;
            }
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox.PasteFromClipboard();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var data = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            data.SetText(Text);

            Clipboard.SetContent(data);
        }

        private static void OnIsReadOnlyPropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((CustomTextBox)sender).UpdateCopyButtons();
        }

        private static void OnAcceptsReturnPropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((CustomTextBox)sender).UpdateCopyButtons();
        }
    }
}
