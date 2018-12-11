using Starcounter;
using Starcounter.Nova;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Key
{
    [Database]
    public class Key1
    {
        public string content { get; internal set; }
    }
}