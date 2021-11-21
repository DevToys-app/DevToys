﻿#nullable enable

using System.Threading.Tasks;
using DevToys.Core.Threading;
using DevToys.Shared.Core;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace DevToys.ViewModels.Tools
{
    internal abstract class ToolProviderBase : ObservableRecipient
    {
        protected const string AssetsFolderPath = "ms-appx:///DevToys/Assets/";

        protected TaskCompletionNotifier<IconElement> CreatePathIconFromPath(string resourceName)
        {
            Arguments.NotNullOrWhiteSpace(resourceName, nameof(resourceName));

            return new TaskCompletionNotifier<IconElement>(
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    string? pathMarkup = Application.Current.Resources[resourceName] as string;
                    Assumes.NotNullOrWhiteSpace(pathMarkup, nameof(pathMarkup));
                    return Task.FromResult<IconElement>(new PathIcon
                    {
                        Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathMarkup)
                    });
                }));
        }
    }
}
