#nullable enable

using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Provides information about a tool and create an instance of it.
    /// </summary>
    public interface IToolProvider
    {
        /// <summary>
        /// Gets the name of the tool that will be displayed in the main menu of the app.
        /// </summary>
        string MenuDisplayName { get; }

        /// <summary>
        /// Gets the name of the tool that will be displayed in the search bar. Sometimes
        /// it is needed to have a different one than the name showed in the menu to increase
        /// result accuracy. For example, while <see cref="MenuDisplayName"/> could be "JSON"
        /// for a tool that is under the Formatter category, <see cref="SearchDisplayName"/>
        /// could be "JSON Formatter", which can be helpful to differentiate from other similar
        /// tools like "JSON Converter".
        /// </summary>
        string? SearchDisplayName { get; }

        /// <summary>
        /// Gets the description of the tool that will be displayed in the tool grid view.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Gets the name of the tool that will be told to the user when using screen reader.
        /// </summary>
        string AccessibleName { get; }

        /// <summary>
        /// Gets the keywords of the tool that are searched in the localized environment.
        /// </summary>
        string? SearchKeywords { get; }

        /// <summary>
        /// Gets an object type that has a width, height and image data. It can be an icon through a font, an SVG...etc.
        /// </summary>
        TaskCompletionNotifier<IconElement> IconSource { get; }

        /// <summary>
        /// Creates a new instance of the tool.
        /// </summary>
        IToolViewModel CreateTool();

        /// <summary>
        /// Analyze the data given in parameter and tells whether the current tool can treat it.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is useful for detecting automatically what tool to suggest the user to use based on an input (coming from the clipboard for example).
        /// </remarks>
        bool CanBeTreatedByTool(string data);
    }
}
