using System.Collections.Generic;
using Templater;

namespace TheElvenScrolls.Wrappers.Abstractions
{
    public interface IToolbox
    {
        IList<string> Justify(string text, int width);
        string JustifyNextLine(ref string left, int width);
        int PredictLength(string text);
        string CreateTemplate(IList<string> text, Template template);
    }
}
