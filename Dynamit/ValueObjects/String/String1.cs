using System.Linq;
using Starcounter;
using static Dynamit.DynamitConfig;

namespace Dynamit.ValueObjects.String
{
    [Database]
    public class String1 : ValueObject
    {
        public string content { get; internal set; }

        public String1(string value)
        {
            content = EscapeStrings ? Escaped(value) : value;
        }

        private static string Escaped(string input) =>
            input.First() == '\"' && input.Last() == '\"'
                ? input.Substring(1, input.Length - 2)
                : input;

        public override string ToString() => content;
    }
}