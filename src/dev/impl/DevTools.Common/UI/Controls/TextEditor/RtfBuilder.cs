#nullable enable

using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;

namespace DevTools.Common.UI.Controls.TextEditor
{
    internal sealed class RtfBuilder
    {
        private Dictionary<Color, int> colorNbs;
        private Dictionary<string, int> fontNbs;

        private StringBuilder sbColorTbl;
        private StringBuilder sbFontTbl;
        private StringBuilder sbText;

        private string defaultFontName;

        public RtfBuilder(ElementTheme theme, string fontName)
        {
            colorNbs = new Dictionary<Color, int>();
            fontNbs = new Dictionary<string, int>();

            sbColorTbl = new StringBuilder();
            sbFontTbl = new StringBuilder();
            sbText = new StringBuilder();

            defaultFontName = fontName;
        }

        public string DefaultFontName
        {
            get
            {
                return defaultFontName;
            }

            set
            {
                defaultFontName = value;
            }
        }

        public void Append(string text, RtfFormat format, Color? color, Color? backgroundColor)
        {
            AppendIntern(text, format, color, backgroundColor, true, null, false);
        }

        public void Clear()
        {
            colorNbs.Clear();
            fontNbs.Clear();

            sbColorTbl.Clear();
            sbFontTbl.Clear();

            sbText.Clear();

            AddFont(DefaultFontName);
        }

        public string GetRtf()
        {
            StringBuilder result = new StringBuilder();

            //header
            result.Append(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1031");

            //font table
            result.Append(@"{\fonttbl");
            //defaultFont
            result.Append(FontToRtf(defaultFontName, 0));
            //user fonts
            if (fontNbs.Count > 0)
            {
                result.Append(sbFontTbl.ToString());
            }
            result.Append("}");

            //color table
            result.Append(@"{\colortbl;");
            //user colors
            if (colorNbs.Count > 0)
            {
                result.Append(sbColorTbl.ToString());
            }
            result.Append("}");

            //text
            result.Append(sbText.ToString());

            //headerclose
            result.Append("}");

            return result.ToString();
        }

        public override string ToString()
        {
            return GetRtf();
        }

        private string EscapeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            else
            {
                text = text.Replace(@"\", @"\\");
                text = text.Replace(@"{", @"\{");
                text = text.Replace(@"}", @"\}");
                text = text.Replace("\r\n", @"\par ");

                return text;
            }
        }

        private int AddColor(Color? color)
        {
            if (!color.HasValue)
            {
                //Default Font
                return 0;
            }
            else
            {
                if (colorNbs.ContainsKey(color.Value))
                {
                    // Font exists already in Fonttable
                    return colorNbs[color.Value];
                }
                else
                {
                    //New Font
                    int nb = colorNbs.Count + 2;

                    colorNbs.Add(color.Value, nb);
                    sbColorTbl.AppendFormat(ColorToRtf(color.Value));

                    return nb;
                }
            }
        }

        private int AddFont(string font)
        {
            if (defaultFontName == font)
            {
                //Default Font
                return 0;
            }
            else
            {
                if (fontNbs.ContainsKey(font))
                {
                    //Font exists alread in font table
                    return fontNbs[font];
                }
                else
                {
                    //new font
                    int fontNb = fontNbs.Count + 1;

                    sbFontTbl.Append(FontToRtf(font, fontNb));
                    fontNbs.Add(font, fontNb);

                    return fontNb;
                }
            }
        }

        private string ColorToRtf(Color color)
        {
            return string.Format(@"\red{0}\green{1}\blue{2};",
                color.R, color.G, color.B);
        }

        private string FontToRtf(string font, int nb)
        {
            return string.Format(@"{{\f{1}\fnil\fcharset0 {0}}}", font, nb);
        }

        private void AppendIntern(string text, RtfFormat format, Color? color, Color? backgroundColor, bool useColor, string font, bool newLine)
        {
            //get color nb
            int colorNb = 1;
            if (useColor)
            {
                colorNb = AddColor(color);
            }

            int backgroundColorNb = 0;
            if (useColor)
            {
                backgroundColorNb = AddColor(backgroundColor);
            }

            //get font
            int fontNb = 0;
            if (font != null)
            {
                fontNb = AddFont(font);
            }

            //set formats
            if ((format & RtfFormat.Bold) == RtfFormat.Bold)
            {
                sbText.Append(@"\b");
            }
            if ((format & RtfFormat.Italic) == RtfFormat.Italic)
            {
                sbText.Append(@"\i");
            }
            if ((format & RtfFormat.Underline) == RtfFormat.Underline)
            {
                sbText.Append(@"\ul");
            }
            if ((format & RtfFormat.Strikeout) == RtfFormat.Strikeout)
            {
                sbText.Append(@"\strike");
            }

            //set color
            sbText.AppendFormat(@"\cf{0}", colorNb);

            //set background color
            sbText.AppendFormat(@"\highlight{0}", backgroundColorNb);

            //set font
            sbText.AppendFormat(@"\f{0}", fontNb);

            //add text
            sbText.Append(EscapeText(text));

            //restore to default color
            sbText.Append(@"\cf1");

            //restore to default font
            sbText.Append(@"\f0");

            //remove formats
            if ((format & RtfFormat.Bold) == RtfFormat.Bold)
            {
                sbText.Append(@"\b0");
            }
            if ((format & RtfFormat.Italic) == RtfFormat.Italic)
            {
                sbText.Append(@"\i0");
            }
            if ((format & RtfFormat.Underline) == RtfFormat.Underline)
            {
                sbText.Append(@"\ul0");
            }
            if ((format & RtfFormat.Strikeout) == RtfFormat.Strikeout)
            {
                sbText.Append(@"\strike0");
            }

            //insert new line
            if (newLine)
            {
                sbText.Append(@"\par ");
            }
        }
    }
}
