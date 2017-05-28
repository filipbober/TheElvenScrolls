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
        private readonly IVariableWidthJustifier _varWidthJustifier;
        private readonly ITemplater _templater;

        public Toolbox(ILogger<Toolbox> logger, IJustifier justifier, IVariableWidthJustifier varWidthJustifier, ITemplater templater)
        {
            _logger = logger;
            _justifier = justifier;
            _varWidthJustifier = varWidthJustifier;
            _templater = templater;

            CheckComponents();
        }

        public IList<string> Justify(string text, int width)
        {
            return _justifier.Justify(text, width);
        }

        public string JustifyNextLine(ref string left, int width)
        {
            return _varWidthJustifier.JustifyNextLine(ref left, width);
        }

        public int PredictLength(string text)
        {
            return _varWidthJustifier.PredictLength(text);
        }

        public string CreateTemplate(IList<string> text, Template template)
        {
            return _templater.Create(text, template);
        }

        private void CheckComponents()
        {
            if (_justifier == null)
                _logger.LogError("Justifier is null");

            if (_varWidthJustifier == null)
                _logger.LogError("VariableWidthJustifier is null");

            if (_templater == null)
                _logger.LogError("Templater is null");
        }
    }
}
