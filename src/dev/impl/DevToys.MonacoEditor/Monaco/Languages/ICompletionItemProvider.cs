#nullable enable

using DevToys.MonacoEditor.Monaco.Editor;
using Newtonsoft.Json;
using Windows.Foundation;

namespace DevToys.MonacoEditor.Monaco.Languages
{
    /// <summary>
    /// The completion item provider interface defines the contract between extensions and
    /// the [IntelliSense](https://code.visualstudio.com/docs/editor/intellisense)./// When computing *complete* completion items is expensive, providers can optionally implement
    /// the `resolveCompletionItem`-function. In that case it is enough to return completion
    /// items with a [label](#CompletionItem.label) from the
    /// [provideCompletionItems](#CompletionItemProvider.provideCompletionItems)-function. Subsequently,
    /// when a completion item is shown in the UI and gains focus this provider is asked to resolve
    /// the item, like adding [doc-comment](#CompletionItem.documentation) or [details](#CompletionItem.detail).
    /// 
    /// </summary>
    public interface ICompletionItemProvider
    {
        [JsonProperty("triggerCharacters", NullValueHandling = NullValueHandling.Ignore)]
        string[] TriggerCharacters { get; }

        /// <summary>
        /// Provide completion items for the given position and document.
        /// </summary>
        IAsyncOperation<CompletionList> ProvideCompletionItemsAsync(IModel model, Position position, CompletionContext context);

        /// <summary>
        /// Given a completion item fill in more data, like [doc-comment](#CompletionItem.documentation)
        /// or [details](#CompletionItem.detail)./// The editor will only resolve a completion item once.
        /// 
        /// </summary>
        IAsyncOperation<CompletionItem> ResolveCompletionItemAsync(IModel model, Position position, CompletionItem item);
    }
}
