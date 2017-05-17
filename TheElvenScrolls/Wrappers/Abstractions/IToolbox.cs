using Templater;

namespace TheElvenScrolls.Wrappers.Abstractions
{
    public interface IToolbox
    {
        string Justify(string text, int width);
        string CreateTemplate(string text, Template template);
    }
}
