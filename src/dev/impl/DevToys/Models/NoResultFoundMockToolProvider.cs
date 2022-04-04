#nullable enable

using System;
using System.ComponentModel;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.Models
{
    internal sealed class NoResultFoundMockToolProvider : IToolProvider
    {
        public string MenuDisplayName => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string? SearchDisplayName => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string? Description => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string AccessibleName => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string? SearchKeywords => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public TaskCompletionNotifier<IconElement> IconSource => null!;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            throw new NotSupportedException();
        }
    }
}
