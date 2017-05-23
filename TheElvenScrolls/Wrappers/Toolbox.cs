using System.Collections.Generic;
using Justifier.Abstractions;
using Microsoft.Extensions.Logging;
using Templater;
using Templater.Abstractions;
using TheElvenScrolls.Wrappers.Abstractions;

namespace TheElvenScrolls.Wrappers
{
    internal sealed class Toolbox : IToolbox
    {
        private readonly ILogger _logger;
        private readonly IJustifier _justifier;
        private readonly ITemplater _templater;

        public Toolbox(ILogger<Toolbox> logger, IJustifier justifier, ITemplater templater)
        {
            _logger = logger;
            _justifier = justifier;
            _templater = templater;
        }

        public IList<string> Justify(string text, int width)
        {
            return _justifier.Justify(text, width);
        }

        public string JustifySingleLine(ref string left, int width)
        {
            return _justifier.JustifySingleLine(ref left, width);
        }

        public int PredictLength(string text)
        {
            return _justifier.PredictLength(text);
        }

        public string CreateTemplate(IList<string> text, Template template)
        {
            return _templater.Create(text, template);
        }
    }
}
