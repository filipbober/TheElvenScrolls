using Templater;

namespace TheElvenScrolls.IO.Abstractions
{
    public interface ITemplateReader
    {
        Template ReadTemplate(string path);
    }
}
