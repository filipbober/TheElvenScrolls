using System.Collections.Generic;

namespace Justifier.Abstractions
{
    public interface IJustifier
    {
        IList<string> Justify(string text, int width);
    }
}
