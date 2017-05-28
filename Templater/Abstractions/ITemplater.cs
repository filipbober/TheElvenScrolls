using System.Collections.Generic;

namespace Templater.Abstractions
{
    public interface ITemplater
    {
        string Create(IList<string> text, Template template);
    }
}
