using DiffPlex;
using System;
using System.Collections.Generic;

namespace DevTools.Common.UI.Controls.FormattedTextBlock
{
    public class CustomDelimiterChunker : IChunker
    {
        private readonly string[] _delimiters;

        public CustomDelimiterChunker(string[] delimiters)
        {
            if (delimiters is null || delimiters.Length == 0)
            {
                throw new ArgumentException($"{nameof(delimiters)} cannot be null or empty.", nameof(delimiters));
            }

            _delimiters = delimiters;
        }

        public string[] Chunk(string inputString)
        {
            var chunks = new List<string>();
            int begin = 0;
            for (int i = 0; i < inputString.Length; i++)
            {
                int delimiterFound = IndexOfAnySubString(inputString, _delimiters, i, out int delimiterLength);
                if (delimiterFound != -1)
                {
                    string word = inputString.Substring(begin, i - begin);
                    string delimiter = inputString.Substring(i, delimiterLength);

                    if (!string.IsNullOrEmpty(word))
                    {
                        chunks.Add(word);
                    }

                    if (!string.IsNullOrEmpty(delimiter))
                    {
                        chunks.Add(delimiter);
                    }

                    begin = i + delimiterLength;
                    i += delimiterLength - 1;
                }
                else if (i == inputString.Length - 1)
                {
                    string lastWord = inputString.Substring(begin, inputString.Length - begin);

                    if (!string.IsNullOrEmpty(lastWord))
                    {
                        chunks.Add(lastWord);
                    }
                }
            }

            return chunks.ToArray();
        }

        private int IndexOfAnySubString(string str, string[] delimiters, int startIndex, out int delimiterLength)
        {
            int bestMatch = -1;
            delimiterLength = 0;

            for (int i = 0; i < delimiters.Length; i++)
            {
                string delimiter = delimiters[i];
                int index = str.IndexOf(delimiter, startIndex);
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
