#nullable enable

using DevToys.MonacoEditor.Monaco.Editor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace DevToys.MonacoEditor.Extensions
{
    internal static class WebViewExtensions
    {

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        static WebViewExtensions()
        {
            _settings.Converters.Add(new AcceptSuggestionOnEnterConverter());
            _settings.Converters.Add(new AccessibilitySupportConverter());
            _settings.Converters.Add(new AutoClosingBracketsConverter());
            _settings.Converters.Add(new AutoClosingOvertypeConverter());
            _settings.Converters.Add(new AutoClosingQuotesConverter());
            _settings.Converters.Add(new AutoFindInSelectionConverter());
            _settings.Converters.Add(new AutoIndentConverter());
            _settings.Converters.Add(new AutoSurroundConverter());
            _settings.Converters.Add(new CursorBlinkingConverter());
            _settings.Converters.Add(new CursorStyleConverter());
            _settings.Converters.Add(new CursorSurroundingLinesStyleConverter());
            _settings.Converters.Add(new FoldingStrategyConverter());
            _settings.Converters.Add(new InsertModeConverter());
            _settings.Converters.Add(new InterfaceToClassConverter<IWordAtPosition, WordAtPosition>());
            _settings.Converters.Add(new LineNumbersTypeConverter());
            _settings.Converters.Add(new MatchBracketsConverter());
            _settings.Converters.Add(new MouseStyleConverter());
            _settings.Converters.Add(new MultiCursorModifierConverter());
            _settings.Converters.Add(new MultiCursorPasteConverter());
            _settings.Converters.Add(new MultipleConverter());
            _settings.Converters.Add(new RenderLineHighlightConverter());
            _settings.Converters.Add(new RenderWhitespaceConverter());
            _settings.Converters.Add(new ScrollbarBehaviorConverter());
            _settings.Converters.Add(new ShowConverter());
            _settings.Converters.Add(new SideConverter());
            _settings.Converters.Add(new SnippetSuggestionsConverter());
            _settings.Converters.Add(new SuggestSelectionConverter());
            _settings.Converters.Add(new TabCompletionConverter());
            _settings.Converters.Add(new WordWrapConverter());
            _settings.Converters.Add(new WrappingIndentConverter());
            _settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter() { NamingStrategy = new CamelCaseNamingStrategy() });
        }

        public static async Task RunScriptAsync(
            this WebView _view,
            string script,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        {
            await _view.RunScriptAsync<object>(script, member, file, line);
        }

        public static async Task<T?> RunScriptAsync<T>(
            this WebView _view,
            string script,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        {
            var start = "try {\n";
            if (typeof(T) != typeof(object))
            {
                script = script.Trim(';');
                start += "JSON.stringify(" + script + ");";
            }
            else
            {
                start += script;
            }
            var fullscript = start +
                "\n} catch (err) { JSON.stringify({ wv_internal_error: true, message: err.message, description: err.description, number: err.number, stack: err.stack }); }";

            if (_view.Dispatcher.HasThreadAccess)
            {
                try
                {
                    return await RunScriptHelperAsync<T>(_view, fullscript);
                }
                catch (Exception e)
                {
                    throw new JavaScriptExecutionException(member, file, line, script, e);
                }
            }
            else
            {
                return await _view.Dispatcher.RunOnUIThreadAsync(async () =>
                {
                    try
                    {
                        return await RunScriptHelperAsync<T>(_view, fullscript);
                    }
                    catch (Exception e)
                    {
                        throw new JavaScriptExecutionException(member, file, line, script, e);
                    }
                });
            }
        }

        private static async Task<T?> RunScriptHelperAsync<T>(WebView _view, string script)
        {
            var returnstring = await _view.InvokeScriptAsync("eval", new string[] { script });

            if (JsonObject.TryParse(returnstring, out JsonObject result))
            {
                if (result.ContainsKey("wv_internal_error") && result["wv_internal_error"].ValueType == JsonValueType.Boolean && result["wv_internal_error"].GetBoolean())
                {
                    throw new JavaScriptInnerException(result["message"].GetString(), result["stack"].GetString());
                }
            }

            if (returnstring != null && returnstring != "null")
            {
                return JsonConvert.DeserializeObject<T>(returnstring);
            }

            return default;
        }

        public static async Task InvokeScriptAsync(
            this WebView _view,
            string method,
            object arg,
            bool serialize = true,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        {
            await _view.InvokeScriptAsync<object>(method, arg, serialize, member, file, line);
        }

        public static async Task InvokeScriptAsync(
            this WebView _view,
            string method,
            object[] args,
            bool serialize = true,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        {
            await _view.InvokeScriptAsync<object>(method, args, serialize, member, file, line);
        }

        public static async Task<T?> InvokeScriptAsync<T>(
            this WebView _view,
            string method,
            object arg,
            bool serialize = true,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        {
            return await _view.InvokeScriptAsync<T>(method, new object[] { arg }, serialize, member, file, line);
        }

        public static async Task<T?> InvokeScriptAsync<T>(
            this WebView _view,
            string method,
            object[] args,
            bool serialize = true,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        {
            string[] sanitizedargs;

            if (serialize)
            {
                sanitizedargs = args.Select(item =>
                {
                    if (item is int || item is double)
                    {
                        return item.ToString();
                    }
                    else if (item is string)
                    {
                        return JsonConvert.ToString(item);
                    }
                    else
                    {
                        // TODO: Need JSON.parse?
                        return JsonConvert.SerializeObject(item, _settings);
                    }
                }).ToArray();
            }
            else
            {
                sanitizedargs = args.Select(item => item.ToString()).ToArray();
            }

            var script = method + "(" + string.Join(",", sanitizedargs) + ");";

            return await RunScriptAsync<T>(_view, script, member, file, line);
        }
    }
}
