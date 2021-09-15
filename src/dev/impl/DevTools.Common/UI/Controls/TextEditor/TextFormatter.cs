#nullable enable

using ColorCode;
using ColorCode.Common;
using ColorCode.Parsing;
using ColorCode.Styling;
using DevTools.Common.UI.Extensions;
using DevTools.Core.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace DevTools.Common.UI.Controls.TextEditor
{
    /// <summary>
    /// Creates a <see cref="TextFormatter"/>, for rendering Syntax Highlighted code to a RichTextBox.
    /// </summary>
    internal sealed class TextFormatter : CodeColorizerBase
    {
        private readonly ITextDocument _textDocument;
        private readonly ITextRange _span;
        private readonly List<(int startPosition, string text, Scope? scope)> _pendingUpdates = new();
        private int _parsedSourceCodePosition;

        /// <summary>
        /// Creates a <see cref="TextFormatter"/>, for rendering Syntax Highlighted code to a RichTextBox.
        /// </summary>
        /// <param name="Theme">The Theme to use, determines whether to use Default Light or Default Dark.</param>
        internal TextFormatter(ElementTheme Theme, ITextDocument textDocument, ITextRange span, ILanguageParser? languageParser = null)
            : base(Theme == ElementTheme.Dark ? StyleDictionary.DefaultDark : StyleDictionary.DefaultLight, languageParser)
        {
            _textDocument = textDocument;
            _span = span;
        }

        /// <summary>
        /// Adds Syntax Highlighted Source Code to the provided RichTextBox.
        /// </summary>
        /// <param name="language">The language to use to colorize the source code.</param>
        /// <param name="textEditor">The Control to add the Text to.</param>
        internal async Task FormatAsync(ILanguage language, CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;

            _span.GetText(TextGetOptions.UseCrlf, out string sourceCode);
            _parsedSourceCodePosition = _span.StartPosition;

            languageParser.Parse(
                sourceCode,
                language,
                (parsedSourceCode, captures) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Write(parsedSourceCode, captures);
                });

            cancellationToken.ThrowIfCancellationRequested();

            const int chunkSize = 5;
            int i = 0;

            while (i < _pendingUpdates.Count)
            {
                var items = _pendingUpdates.Skip(i).Take(chunkSize);
                i += chunkSize;

                cancellationToken.ThrowIfCancellationRequested();

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Low,
                    () =>
                    {
                        foreach (var item in items)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            ApplyStyle(item.startPosition, item.text, item.scope);
                        }
                    });
            }
        }

        protected override void Write(string parsedSourceCode, IList<Scope> scopes)
        {
            var styleInsertions = new List<TextInsertion>();

            foreach (Scope scope in scopes)
            {
                GetStyleInsertionsForCapturedStyle(scope, styleInsertions);
            }

            styleInsertions.SortStable((x, y) => x.Index.CompareTo(y.Index));

            int parsedSourceCodePosition = _parsedSourceCodePosition;
            int offset = 0;

            Scope? previousScope = null;

            foreach (TextInsertion? styleinsertion in styleInsertions)
            {
                string text = parsedSourceCode.Substring(offset, styleinsertion.Index - offset);
                if (!string.IsNullOrEmpty(text))
                {
                    _pendingUpdates.Add(new(parsedSourceCodePosition + offset, text, previousScope));
                }

                if (!string.IsNullOrWhiteSpace(styleinsertion.Text))
                {
                    _pendingUpdates.Add(new(parsedSourceCodePosition, text, previousScope));
                }

                offset = styleinsertion.Index;
                previousScope = styleinsertion.Scope;
            }

            string remaining = parsedSourceCode.Substring(offset);

            // Ensures that those loose carriages don't run away!
            if (!string.IsNullOrEmpty(remaining) && remaining != "\r")
            {
                _pendingUpdates.Add(new(parsedSourceCodePosition + offset, remaining, null));
            }

            _parsedSourceCodePosition += parsedSourceCode.Length - CountStringOccurrences(parsedSourceCode, "\r");
        }

        private void ApplyStyle(int startPosition, string text, Scope? scope)
        {
            ITextRange span = _textDocument.GetRange(startPosition, startPosition + text.Length);
            ColorCode.Styling.Style style;

            if (scope is not null && Styles.Contains(scope.Name))
            {
                style = Styles[scope.Name];
            }
            else
            {
                style = Styles["Plain Text"]; // Default
            }

            string? foreground = style.Foreground;
            bool italic = style.Italic;
            bool bold = style.Bold;

            if (!string.IsNullOrWhiteSpace(foreground))
            {
                span.CharacterFormat.ForegroundColor = foreground!.ToColor();
            }

            if (italic)
            {
                span.CharacterFormat.FontStyle = FontStyle.Italic;
            }

            if (bold)
            {
                span.CharacterFormat.Bold = FormatEffect.On;
            }
        }

        private void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions)
        {
            styleInsertions.Add(new TextInsertion
            {
                Index = scope.Index,
                Scope = scope
            });

            foreach (Scope childScope in scope.Children)
            {
                GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);
            }

            styleInsertions.Add(new TextInsertion
            {
                Index = scope.Index + scope.Length
            });
        }

        /// <summary>
        /// Count occurrences of strings.
        /// </summary>
        private static int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }
    }
}
