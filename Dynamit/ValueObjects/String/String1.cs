using System.Linq;
using Starcounter;

namespace Dynamit.ValueObjects.String
{
    [Database]
    public class String1 : ValueObject
    {
        private string _content;

        public string content
        {
            get { return _content; }
            set
            {
                var s = value;
                if (DynamitConfig.EscapeStrings && s.First() == '\"' && s.Last() == '\"')
                    s = s.Substring(1, s.Length - 2);
                _content = s;
            }
        }

        public override string ToString() => content;
    }
}