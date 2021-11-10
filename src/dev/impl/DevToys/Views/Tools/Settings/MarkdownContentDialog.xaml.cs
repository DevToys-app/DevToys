#nullable enable

using System;
using Windows.UI.Xaml.Controls;

namespace DevToys.Views.Tools.Settings
{
    public sealed partial class MarkdownContentDialog : UserControl
    {
        public MarkdownContentDialog(string markdown)
        {
            InitializeComponent();

            MarkdownTextBlock.Text = markdown;
        }

        private async void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            try
            {
                string? uriToLaunch = e.Link;
                var uri = new Uri(uriToLaunch);

                await Windows.System.Launcher.LaunchUriAsync(uri);
            }
            catch { }
        }
    }
}
