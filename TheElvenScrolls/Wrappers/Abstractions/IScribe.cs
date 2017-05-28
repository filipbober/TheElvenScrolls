using Templater;

namespace TheElvenScrolls.Wrappers.Abstractions
{
    public interface IScribe
    {
        string ReadInput(string path);
        Template ReadTemplate(string path);
        void WriteScroll(string path, string scroll);
    }
}
