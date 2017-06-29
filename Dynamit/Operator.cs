using System;

#pragma warning disable 1591

namespace Dynamit
{
    public enum Operators
    {
        EQUALS,
        NOT_EQUALS,
    }

    /// <summary>
    /// Encodes an equality operator, used in conditions. Can be implicitly converted
    /// from string ("=" | "!=").
    /// </summary>
    public struct Operator
    {
        /// <summary>
        /// The code for this operator
        /// </summary>
        public readonly Operators OpCode;

        internal string Common => GetString(OpCode);
        internal bool Equality => OpCode == Operators.EQUALS || OpCode == Operators.NOT_EQUALS;
        internal bool Compare => !Equality;
        internal string SQL => OpCode == Operators.NOT_EQUALS ? "<>" : Common;
        public override bool Equals(object obj) => obj is Operator && (Operator) obj == OpCode;
        public bool Equals(Operator other) => OpCode == other.OpCode;
        public override int GetHashCode() => (int) OpCode;
        public override string ToString() => Common;
        public static bool operator ==(Operator o1, Operator o2) => o1.OpCode == o2.OpCode;
        public static bool operator !=(Operator o1, Operator o2) => !(o1 == o2);

        public static Operator EQUALS => Operators.EQUALS;
        public static Operator NOT_EQUALS => Operators.NOT_EQUALS;

        internal Operator(Operators op) => OpCode = op;

        internal static string GetString(Operators op)
        {
            switch (op)
            {
                case Operators.EQUALS: return "=";
                case Operators.NOT_EQUALS: return "!=";
                default: throw new ArgumentOutOfRangeException();
            }
        }

        internal static Operator Parse(string common)
        {
            switch (common.Trim())
            {
                case "=": return new Operator(Operators.EQUALS);
                case "!=": return new Operator(Operators.NOT_EQUALS);
                default:
                    throw new ArgumentException($"Cannot convert '{common}' to Dynamit.Operator. " +
                                                "Only '=' and '!=' are allowed in Finder queries. " +
                                                "To query using other operators, use LINQ statements " +
                                                "on the returned entities");
            }
        }

        internal static bool TryParse(string common, out Operator op)
        {
            try
            {
                op = Parse(common);
                return true;
            }
            catch
            {
                op = default(Operator);
                return false;
            }
        }

        /// <summary>
        /// Implicitly converts a string to a Dynamit.Operator (if string is ("=" | "!=")).
        /// </summary>
        /// <param name="op">The string to convert</param>
        public static implicit operator Operator(string op) => TryParse(op, out Operator @operator)
            ? @operator
            : throw new ArgumentException(nameof(op));

        public static implicit operator Operator(Operators op) => new Operator(op);
    }
}