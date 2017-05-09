using Templater;

namespace TheElvenScrolls
{
    public interface ITemplateReader
    {
        Template ReadTemplate(string path);
    }
}
