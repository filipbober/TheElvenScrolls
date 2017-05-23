using System.Collections.Generic;

namespace Justifier.Abstractions
{
    public interface IJustifier
    {
        IList<string> Justify(string text, int width);
        string JustifySingleLine(ref string left, int width);
        int PredictLength(string text);
    }
}
