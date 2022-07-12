#nullable enable

using System;
using System.Collections.Generic;

namespace DevToys.Helpers.SqlFormatter.Core
{
    /// <summary>
    /// Bookkeeper for inline blocks.
    /// Inline blocks are parenthized expressions that are shorter than INLINE_MAX_LENGTH.
    /// These blocks are formatted on a single line, unlike longer parenthized expressions
    /// where open-parenthesis causes newline and increase of indentation.
    /// </summary>
    internal sealed class InlineBlock
    {
        private const int InlineMaxLength = 50;

        private int _level = 0;

        /// <summary>
        /// Begins inline block when lookahead through upcoming tokens determines that the
        /// block would be smaller than INLINE_MAX_LENGTH.
        /// </summary>
        /// <param name="tokens">Array of all tokens</param>
        /// <param name="index">Current token position</param>
        internal void BeginIfPossible(IReadOnlyList<Token> tokens, int index, ReadOnlySpan<char> valueSpan)
        {
            if (_level == 0 && IsInlineBlock(tokens, index, valueSpan))
            {
                _level = 1;
            }
            else if (_level > 0)
            {
                _level++;
            }
            else
            {
                _level = 0;
            }
        }

        /// <summary>
        /// Finishes current inline block. There might be several nested ones.
        /// </summary>
        internal void End()
        {
            _level--;
        }

        /// <summary>
        /// True when inside an inline block
        /// </summary>
        internal bool IsActive()
        {
            return _level > 0;
        }

        /// <summary>
        /// Check if this should be an inline parentheses block.
        /// Examples are "NOW()", "COUNT(*)", "int(10)", key(`somecolumn`), DECIMAL(7,2)
        /// </summary>
        private bool IsInlineBlock(IReadOnlyList<Token> tokens, int index, ReadOnlySpan<char> valueSpan)
        {
            int length = 0;
            int level = 0;

            for (int i = index; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                length += token.Length;

                // Overran max length
                if (length > InlineMaxLength)
                {
                    return false;
                }

                if (token.Type == TokenType.OpenParen)
                {
                    level++;
                }
                else if (token.Type == TokenType.CloseParen)
                {
                    level--;
                    if (level == 0)
                    {
                        return true;
                    }
                }

                if (IsForbiddenToken(token, valueSpan))
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Reserved words that cause newlines, comments and semicolons are not allowed inside inline parentheses block
        /// </summary>
        private bool IsForbiddenToken(Token token, ReadOnlySpan<char> valueSpan)
        {
            return
                token.Type == TokenType.ReservedTopLevel
                || token.Type == TokenType.ReservedNewLine
                // || token.Type == TokenType.LineComment
                || token.Type == TokenType.BlockComment
                || (token.Length == 1 && valueSpan[0] == ';');
        }
    }
}
