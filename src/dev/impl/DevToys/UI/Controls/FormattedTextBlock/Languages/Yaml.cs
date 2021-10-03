#nullable enable

using ColorCode;
using ColorCode.Common;
using System.Collections.Generic;

namespace DevToys.UI.Controls.FormattedTextBlock.Languages
{
    internal sealed class Yaml : ILanguage
    {
        public string Id
        {
            get { return "yaml"; }
        }

        public string Name
        {
            get { return "YAML"; }
        }

        public string CssClassName
        {
            get { return "yaml"; }
        }

        public string FirstLinePattern
        {
            get
            {
                return null;
            }
        }

        public IList<LanguageRule> Rules
        {
            get
            {
                return new List<LanguageRule>
                           {
                               new LanguageRule(
                                   @"(#.*?)\r?$",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new LanguageRule(
                                   @"'(?>[^\']*)'",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new LanguageRule(
                                   @"""(?>[^\""]*)""",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new LanguageRule(
                                   @"^[^:]+",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Keyword },
                                       })
                           };
            }
        }

        public bool HasAlias(string lang)
        {
            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
