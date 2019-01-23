using System.Text;
using Starcounter.Nova;
using static Dynamit.DynamitConfig;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.String
{
    [Database]
    public class String1 : ValueObject
    {
        public string content { get; internal set; }
        internal override long ByteCount => Encoding.UTF8.GetByteCount(content);
        public String1(string value) => content = EscapeStrings ? Escaped(value) : value;
        public override string ToString() => content;

        private static string Escaped(string input)
        {
            switch (input)
            {
                case null:
                case "":
                case "\"": return input;
                case var _ when input[0] == '\"' && input[input.Length - 1] == '\"':
                    return input.Substring(1, input.Length - 2);
                default: return input;
            }
        }
    }
}