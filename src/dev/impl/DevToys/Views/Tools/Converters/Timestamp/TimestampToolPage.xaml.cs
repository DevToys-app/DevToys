#nullable enable

using System;
using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.Timestamp;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.Timestamp
{
    public sealed partial class TimestampToolPage : Page
    {
        public readonly static double MinimumTimestamp = (double)long.MinValue;
        public readonly static double MaximumTimestamp = (double)long.MaxValue;

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(TimestampToolViewModel),
                typeof(TimestampToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public TimestampToolViewModel ViewModel
        {
            get => (TimestampToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public TimestampToolPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            if (ViewModel is null)
            {
                // Set the view model
                Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
                ViewModel = (TimestampToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            if (!string.IsNullOrWhiteSpace(parameters.ClipBoardContent))
            {
                if (long.TryParse(parameters.ClipBoardContent, out long timestamp))
                {
                    ViewModel.Timestamp = timestamp;
                }
                else if (DateTime.TryParse(parameters.ClipBoardContent, out DateTime localDateTime))
                {
                    ViewModel.Timestamp = new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds();
                }
            }

            base.OnNavigatedTo(e);
        }
    }
}
