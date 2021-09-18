#nullable enable

using DevTools.Common;
using DevTools.Common.UI.Controls.FormattedTextBlock;
using DevTools.Core;
using DevTools.Core.Settings;
using DevTools.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;

namespace DevTools.Providers.Impl.Tools.TextDiff
{
    [Export(typeof(TextDiffToolViewModel))]
    public sealed class TextDiffToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly Queue<(string? oldText, string? newText)> _comparisonQueue = new();
        private readonly IThread _thread;

        private bool _comparisonInProgress;
        private string? _oldText;
        private string? _newText;

        public Type View { get; } = typeof(TextDiffToolPage);

        internal ISettingsProvider SettingsProvider { get; }

        internal TextDiffStrings Strings => LanguageManager.Instance.TextDiff;

        internal string? OldText
        {
            get => _oldText;
            set
            {
                SetProperty(ref _oldText, value, broadcast: true);
                QueueComparison();
                FormattedTextBlock?.SetInlineTextDiff(value, NewText, lineDiff: false);
            }
        }

        internal string? NewText
        {
            get => _newText;
            set
            {
                SetProperty(ref _newText, value, broadcast: true);
                QueueComparison();
                FormattedTextBlock?.SetInlineTextDiff(OldText, value, lineDiff: false);
            }
        }

        internal IFormattedTextBlock? FormattedTextBlock { private get; set; }

        [ImportingConstructor]
        public TextDiffToolViewModel(ISettingsProvider settingsProvider, IThread thread)
        {
            SettingsProvider = settingsProvider;
            _thread = thread;
        }

        private void QueueComparison()
        {
            _comparisonQueue.Enqueue(new(OldText, NewText));
            TreatComparisonQueueAsync().Forget();
        }

        private async Task TreatComparisonQueueAsync()
        {
            if (_comparisonInProgress)
            {
                return;
            }

            _comparisonInProgress = true;

            Assumes.NotNull(FormattedTextBlock, nameof(FormattedTextBlock));

            await TaskScheduler.Default;

            while (_comparisonQueue.TryDequeue(out (string? oldText, string? newText) item))
            {
                //   await TextEditor!.ShowTextDiffAsync(item.oldText, item.newText);
            }

            _comparisonInProgress = false;
        }
    }
}
