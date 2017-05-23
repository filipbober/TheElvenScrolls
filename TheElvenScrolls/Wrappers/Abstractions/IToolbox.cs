using Templater;

namespace TheElvenScrolls.Wrappers.Abstractions
{
    public interface IToolbox
    {
        string Justify(string text, int width);
        string JustifySingleLine(ref string left, int width);
        int PredictLength(string text);
        string CreateTemplate(string text, Template template);
    }
}
