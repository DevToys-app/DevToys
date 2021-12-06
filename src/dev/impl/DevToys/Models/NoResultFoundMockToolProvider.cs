#nullable enable

using System;
using System.ComponentModel;
using DevToys.Api.Tools;

namespace DevToys.Models
{
    internal sealed class NoResultFoundMockToolProvider : IToolProvider
    {
        public string DisplayName => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string AccessibleName => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public object IconSource => null!;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            // TODO: Show a page indicating "No results match your search".
            throw new NotSupportedException();
        }
    }
}
