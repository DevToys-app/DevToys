using DiffPlex;
using System;
using System.Collections.Generic;

namespace DevTools.Common.UI.Controls.FormattedTextBlock
{
    public class CustomDelimiterChunker : IChunker
    {
        private readonly string[] delimiters;

        public CustomDelimiterChunker(string[] delimiters)
        {
            if (delimiters is null || delimiters.Length == 0)
            {
                throw new ArgumentException($"{nameof(delimiters)} cannot be null or empty.", nameof(delimiters));
            }

            this.delimiters = delimiters;
        }

        public string[] Chunk(string str)
        {
            var list = new List<string>();
            int begin = 0;
            for (int i = 0; i < str.Length; i++)
            {
                var delimiterFound = IndexOfAnySubString(str, delimiters, i, out int delimiterLength);
                if (delimiterFound != -1)
                {
                    string word = str.Substring(begin, i - begin);
                    string delimiter = str.Substring(i, delimiterLength);

                    if (!string.IsNullOrEmpty(word))
                    {
                        list.Add(word);
                    }

                    if (!string.IsNullOrEmpty(delimiter))
                    {
                        list.Add(delimiter);
                    }

                    begin = i + delimiterLength;
                    i += delimiterLength - 1;
                }
                else if (i == str.Length - 1)
                {
                    string lastWord = str.Substring(begin, str.Length - begin);

                    if (!string.IsNullOrEmpty(lastWord))
                    {
                        list.Add(lastWord);
                    }
                }
            }

            return list.ToArray();
        }

        private int IndexOfAnySubString(string str, string[] delimiters, int startIndex, out int delimiterLength)
        {
            var bestMatch = -1;
            delimiterLength = 0;

            for (int i = 0; i < delimiters.Length; i++)
            {
                string delimiter = delimiters[i];
                var index = str.IndexOf(delimiter, startIndex);
                if (index == startIndex && delimiter.Length > delimiterLength)
                {
                    bestMatch = index;
                    delimiterLength = delimiter.Length;
                }
            }

            return bestMatch;
        }
    }
}
