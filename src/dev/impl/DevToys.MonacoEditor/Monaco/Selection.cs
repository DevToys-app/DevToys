#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// A selection in the editor.
    /// The selection is a range that has an orientation.
    /// </summary>
    public sealed class Selection : IRange
    {
        /// <summary>
        /// Line number on which the range starts (starts at 1).
        /// </summary>
        [JsonProperty("startLineNumber")]
        public uint StartLineNumber { get; private set; }

        /// <summary>
        /// Column on which the range starts in line `startLineNumber` (starts at 1).
        /// </summary>
        [JsonProperty("startColumn")]
        public uint StartColumn { get; private set; }

        /// <summary>
        /// Line number on which the range ends.
        /// </summary>
        [JsonProperty("endLineNumber")]
        public uint EndLineNumber { get; private set; }

        /// <summary>
        /// Column on which the range ends in line `endLineNumber`.
        /// </summary>
        [JsonProperty("endColumn")]
        public uint EndColumn { get; private set; }

        /// <summary>
        /// The line number on which the selection has ended.
        /// </summary>
        [JsonProperty("positionLineNumber")]
        public uint PositionLineNumber { get; private set; }

        /// <summary>
        /// The column on `positionLineNumber` where the selection has ended.
        /// </summary>
        [JsonProperty("positionColumn")]
        public uint PositionColumn { get; private set; }

        /// <summary>
        /// The line number on which the selection has started.
        /// </summary>
        [JsonProperty("selectionStartLineNumber")]
        public uint SelectionStartLineNumber { get; private set; }

        /// <summary>
        /// The column on `selectionStartLineNumber` where the selection has started.
        /// </summary>
        [JsonProperty("selectionStartColumn")]
        public uint SelectionStartColumn { get; private set; }

        [JsonIgnore]
        public SelectionDirection Direction { get; private set; }

        public Selection(uint selectionStartLineNumber, uint selectionStartColumn, uint positionLineNumber, uint positionColumn)
        {
            SelectionStartLineNumber = selectionStartLineNumber;
            SelectionStartColumn = selectionStartColumn;
            PositionLineNumber = positionLineNumber;
            PositionColumn = positionColumn;

            if (selectionStartLineNumber < positionLineNumber
                || (selectionStartLineNumber == positionLineNumber && selectionStartColumn <= positionColumn))
            {
                // Start is first
                StartLineNumber = SelectionStartLineNumber;
                StartColumn = SelectionStartColumn;
                EndLineNumber = PositionLineNumber;
                EndColumn = PositionColumn;

                Direction = SelectionDirection.LTR;
            }
            else
            {
                // Flipped
                StartLineNumber = PositionLineNumber;
                StartColumn = PositionColumn;
                EndLineNumber = SelectionStartLineNumber;
                EndColumn = SelectionStartColumn;

                Direction = SelectionDirection.RTL;
            }
        }

        public SelectionDirection GetDirection()
        {
            return Direction;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}-> {2}, {3}]", SelectionStartLineNumber, SelectionStartColumn, PositionLineNumber, PositionColumn);
        }
    }
}
