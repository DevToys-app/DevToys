using DevToys.MonacoEditor.Monaco.Editor;
using System.Text;

namespace DevToys.MonacoEditor.Monaco.Helpers;

/// <summary>
/// Broker to help manage CSS Styles and their usage within each CodeEditor. Lifetime managed by CodeEditor.
/// </summary>
internal sealed class CssStyleBroker : IDisposable
{
    private static uint id = 0;

    // Track styles registered to this particular editor.
    private static readonly Dictionary<uint, WeakReference<ICssStyle>> registry = new();

    private static readonly Dictionary<WeakReference<CodeEditor>, HashSet<uint>> knownStyles = new();

    private static readonly Dictionary<WeakReference<CodeEditor>, bool> isDirty = new();

    private readonly WeakReference<CodeEditor> _parent;

    public CssStyleBroker(CodeEditor codeEditor)
    {
        _parent = new WeakReference<CodeEditor>(codeEditor);
        knownStyles.Add(_parent, new HashSet<uint>());
        isDirty.Add(_parent, false);
    }

    public void Dispose()
    {
        knownStyles.Remove(_parent);
        isDirty.Remove(_parent);
    }

    /// <summary>
    /// Returns the name for a style to use after registered. Generates a unique style name.
    /// </summary>
    /// <param name="style"></param>
    /// <returns></returns>
    public static uint Register(ICssStyle style)
    {
        uint id = CssStyleBroker.id++;
        registry.Add(id, new WeakReference<ICssStyle>(style));
        return id;
    }

    public bool AssociateStyles(IReadOnlyList<IModelDeltaDecoration> decorations)
    {
        /// By construction we assume that decorations will not be null from the call in <see cref="CodeEditor.DeltaDecorationsHelperAsync"/>
        bool newStyle = isDirty[_parent]; /// Can be set in <see cref="GetStyles"/>.

        isDirty[_parent] = false; // Reset

        for (int i = 0; i < decorations.Count; i++)
        {
            IModelDeltaDecoration decoration = decorations[i];

            // Add (or ignore) elements to the collection.
            // If any Adds are new, we flag our boolean to return
            if (decoration.Options.ClassName != null)
            {
                newStyle |= knownStyles[_parent].Add(decoration.Options.ClassName.Id);
            }
            if (decoration.Options.GlyphMarginClassName != null)
            {
                newStyle |= knownStyles[_parent].Add(decoration.Options.GlyphMarginClassName.Id);
            }
            if (decoration.Options.InlineClassName != null)
            {
                newStyle |= knownStyles[_parent].Add(decoration.Options.InlineClassName.Id);
            }
        }

        return newStyle;
    }

    /// <summary>
    /// Returns the CSS block for all registered styles.
    /// </summary>
    /// <returns></returns>
    public string GetStyles()
    {
        var rules = new StringBuilder(100);
        knownStyles[_parent].RemoveWhere(id =>
        {
            if (registry[id].TryGetTarget(out ICssStyle? style))
            {
                rules.AppendLine(style.ToCss());
            }
            else
            {
                // Clean-up from disposed style objects
                foreach (KeyValuePair<WeakReference<CodeEditor>, HashSet<uint>> entry in knownStyles)
                {
                    if (entry.Key == _parent)
                    {
                        break; // Skip our current editor, as we can't remove from within the loop. Thus the return true below in this RemoveWhere clause.
                    }
                    entry.Value.Remove(id); // Remove from Style set
                    isDirty[entry.Key] = true; // Mark that editor as dirty
                }
                registry.Remove(id); // Remove the style completely from our known world as it's gone.

                return true; // Also remove this entry from our own set.
            }

            return false; // Default, we're just using this as a dumb loop, but that we can remove from when needed above.
        });

        return rules.ToString();
    }
}
