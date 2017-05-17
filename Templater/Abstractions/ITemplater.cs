namespace Templater.Abstractions
{
    public interface ITemplater
    {
        string Create(string text, Template template);
    }
}
