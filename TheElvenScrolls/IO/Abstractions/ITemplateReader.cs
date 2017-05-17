using Templater;

namespace TheElvenScrolls.IO.Abstractions
{
    public interface ITemplateReader
    {
        Template Read(string path);
    }
}
