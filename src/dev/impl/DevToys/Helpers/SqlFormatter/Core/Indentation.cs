#nullable enable

using System;
using System.Collections.Generic;

namespace DevToys.Helpers.SqlFormatter.Core
{
    /// <summary>
    /// Manages indentation levels.
    /// </summary>
    internal sealed class Indentation
    {
        private enum IndentationType
        {
            /// <summary>
            /// increased by ReservedTopLevel words
            /// </summary>
            TopLevel,

            /// <summary>
            /// increased by open-parenthesis
            /// </summary>
            BlockLevel
        }

        private readonly Stack<IndentationType> _indentationTypes = new();
        private readonly Models.Indentation _indentation;

        public Indentation(Models.Indentation indentation)
        {
            _indentation = indentation;
        }

        /// <summary>
        /// Returns current indentation string.
        /// </summary>
        internal string GetIndent()
        {
            return _indentation switch
            {
                Models.Indentation.TwoSpaces => new string(' ', 2 * _indentationTypes.Count),
                Models.Indentation.FourSpaces => new string(' ', 4 * _indentationTypes.Count),
                Models.Indentation.OneTab => new string('\t', 1 * _indentationTypes.Count),
                _ => throw new NotSupportedException(),
            };
        }

        /// <summary>
        /// Increases indentation by one top-level indent.
        /// </summary>
        internal void IncreaseTopLevel()
        {
            _indentationTypes.Push(IndentationType.TopLevel);
        }

        /// <summary>
        /// Increases indentation by one block-level indent.
        /// </summary>
        internal void IncreaseBlockLevel()
        {
            _indentationTypes.Push(IndentationType.BlockLevel);
        }

        /// <summary>
        /// Decreases indentation by one top-level indent.
        /// Does nothing when the previous indent is not top-level.
        /// </summary>
        internal void DecreaseTopLevel()
        {
            if (_indentationTypes.TryPeek(out IndentationType type) && type == IndentationType.TopLevel)
            {
                _indentationTypes.Pop();
            }
        }

        /// <summary>
        /// Decreases indentation by one block-level indent.
        /// If there are top-level indents within the block-level indent, throws away these as well.
        /// </summary>
        internal void DecreaseBlockLevel()
        {
            while (_indentationTypes.Count > 0)
            {
                IndentationType type = _indentationTypes.Pop();
                if (type != IndentationType.TopLevel)
                {
                    break;
                }
            }
        }

        internal void ResetIndentation()
        {
            _indentationTypes.Clear();
        }
    }
}
