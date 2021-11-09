#nullable enable

using System;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Metadata;

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// A position in the editor.
    /// </summary>
    public sealed class Position : IPosition
    {
        /// <summary>
        /// column (the first character in a line is between column 1 and column 2)
        /// </summary>
        [JsonProperty("column")]
        public uint Column { get; private set; }

        /// <summary>
        /// line number (starts at 1)
        /// </summary>
        [JsonProperty("lineNumber")]
        public uint LineNumber { get; private set; }

        public Position(uint lineNumber, uint column)
        {
            Column = column;
            LineNumber = lineNumber;
        }

        public Position Clone()
        {
            return new Position(LineNumber, Column);
        }

        public override bool Equals(object obj)
        {
            if (obj is Position other)
            {
                return LineNumber == other.LineNumber && Column == other.Column;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return new Point(LineNumber, Column).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", LineNumber, Column);
        }

        public bool IsBefore(Position other)
        {
            // TODO:
            throw new NotImplementedException();
        }

        public bool IsBeforeOrEqual(Position other)
        {
            // TODO:
            throw new NotImplementedException();
        }

        [DefaultOverload]
        public int CompareTo(Position other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object obj)
        {
            if (obj is IPosition position)
            {
                return CompareTo(Lift(position));
            }

            throw new NotImplementedException();
        }

        public static int Compare(Position a, Position b)
        {
            return a.CompareTo(b);
        }

        // Can't Export static Method with same name in Windows Runtime Component
        /*public static bool Equals(Position a, Position b)
        {
            return a.Equals(b);
        }

        public static bool IsBefore(Position a, Position b)
        {
            return a.IsBefore(b);
        }

        public static bool IsBeforeOrEqual(Position a, Position b)
        {
            return a.IsBeforeOrEqual(b);
        }*/

        public static bool IsIPosition(object a)
        {
            return a is Position;
        }

        public static Position Lift(IPosition pos)
        {
            return new Position(pos.LineNumber, pos.Column);
        }
    }
}
