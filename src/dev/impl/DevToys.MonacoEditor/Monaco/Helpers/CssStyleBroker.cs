#nullable enable

using System.Collections.Generic;
using System.Text;

namespace DevToys.MonacoEditor.Monaco.Helpers
{
    /// <summary>
    /// Singleton Broker to help us manage CSS Styles
    /// </summary>
    public sealed class CssStyleBroker
    {
        private static uint Id = 0;
        private readonly Dictionary<string, ICssStyle> _registered = new Dictionary<string, ICssStyle>();
        private static readonly IDictionary<CodeEditor, CssStyleBroker> instances = new Dictionary<CodeEditor, CssStyleBroker>();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static CssStyleBroker()
        {
        }

        private CssStyleBroker()
        {
        }

        public static CssStyleBroker GetInstance(CodeEditor editor)
        {
            if (instances.ContainsKey(editor))
            {
                return instances[editor];
            }

            return instances[editor] = new CssStyleBroker();
        }

        public static bool DetachEditor(CodeEditor editor)
        {
            if (instances.ContainsKey(editor))
            {
                return instances.Remove(editor);
            }

            return true;
        }

        /// <summary>
        /// Returns the name for a style to use after registered.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public string Register(ICssStyle style)
        {
            Id += 1;
            var name = "generated-style-" + Id;
            _registered.Add(name, style);
            return name;
        }

        /// <summary>
        /// Returns the CSS block for all registered styles.
        /// </summary>
        /// <returns></returns>
        public string GetStyles()
        {
            StringBuilder rules = new StringBuilder(100);
            foreach (ICssStyle css in _registered.Values)
            {
                rules.AppendLine(css.ToCss());
            }
            return rules.ToString();
        }

        public static string WrapCssClassName(ICssStyle style, string inner)
        {
            return string.Format(".{0} {{ {1} }}", style.Name, inner);
        }
    }
}
