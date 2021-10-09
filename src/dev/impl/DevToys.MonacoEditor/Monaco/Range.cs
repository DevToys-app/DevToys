#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// A range in the editor. (startLineNumber,startColumn) is &lt;= (endLineNumber,endColumn)
    /// </summary>
    public sealed class Range : IRange
    {
        /// <summary>
        /// Column on which the range ends in line `endLineNumber`.
        /// </summary>
        [JsonProperty("endColumn")]
        public uint EndColumn { get; private set; }

        /// <summary>
        /// Line number on which the range ends.
        /// </summary>
        [JsonProperty("endLineNumber")]
        public uint EndLineNumber { get; private set; }

        /// <summary>
        /// Column on which the range starts in line `startLineNumber` (starts at 1).
        /// </summary>
        [JsonProperty("startColumn")]
        public uint StartColumn { get; private set; }

        /// <summary>
        /// Line number on which the range starts (starts at 1).
        /// </summary>
        [JsonProperty("startLineNumber")]
        public uint StartLineNumber { get; private set; }

        public Range(uint startLineNumber, uint startColumn, uint endLineNumber, uint endColumn)
        {
            // TODO: Range Check? Monaco doesn't seem to do it currently...
            StartLineNumber = startLineNumber;
            StartColumn = startColumn;
            EndLineNumber = endLineNumber;
            EndColumn = endColumn;
        }

        public Range CloneRange()
        {
            return new Range(StartLineNumber, StartColumn, EndLineNumber, EndColumn);
        }

        public Range CollapseToStart()
        {
            return new Range(StartColumn, StartColumn, StartLineNumber, StartColumn);
        }

        public Range ContainsPosition(IPosition position)
        {
            // TODO
            throw new NotImplementedException();
        }

        public bool ContainsRange(IRange range)
        {
            // TODO
            throw new NotImplementedException();
        }

        public bool EqualsRange(Range other)
        {
            return StartColumn == other.StartColumn &&
                    StartLineNumber == other.StartLineNumber &&
                    EndColumn == other.EndColumn &&
                    EndLineNumber == other.EndLineNumber;
        }

        public Position GetEndPosition()
        {
            return new Position(EndLineNumber, EndColumn);
        }

        public Position GetStartPosition()
        {
            return new Position(StartLineNumber, StartColumn);
        }

        public Range IntersectRanges(IRange range)
        {
            // TODO
            throw new NotImplementedException();
        }

        public bool IsEmpty()
        {
            return StartLineNumber == EndLineNumber && StartColumn == EndColumn;
        }

        public Range PlusRange(IRange range)
        {
            // TODO
            throw new NotImplementedException();
        }

        public Range SetEndPosition(uint endLineNumber, uint endColumn)
        {
            return new Range(StartLineNumber, StartColumn, endLineNumber, endColumn);
        }

        public Range SetStartPosition(uint startLineNumber, uint startColumn)
        {
            return new Range(startLineNumber, startColumn, EndLineNumber, EndColumn);
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}-> {2}, {3}]", StartLineNumber, StartColumn, EndLineNumber, EndColumn);
        }

        public static Range Lift(IRange range)
        {
            return new Range(range.StartLineNumber, range.StartColumn, range.EndLineNumber, range.EndColumn);
        }

        // TODO: Weed out unique static method to put here.
    }
}
