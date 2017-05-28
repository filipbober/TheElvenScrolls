using Microsoft.Extensions.Logging;
using Templater;
using TheElvenScrolls.IO.Abstractions;
using TheElvenScrolls.Wrappers.Abstractions;

namespace TheElvenScrolls.Wrappers
{
    internal sealed class Scribe : IScribe
    {
        private readonly ILogger _logger;
        private readonly IInputReader _inputReader;
        private readonly ITemplateReader _templateReader;
        private readonly IScrollWriter _scrollWriter;

        public Scribe(ILogger<Scribe> logger, IInputReader inputReader, ITemplateReader templateReader, IScrollWriter scrollWriter)
        {
            _logger = logger;
            _inputReader = inputReader;
            _templateReader = templateReader;
            _scrollWriter = scrollWriter;
        }

        public string ReadInput(string path)
        {
            return _inputReader.Read(path);
        }

        public Template ReadTemplate(string path)
        {
            return _templateReader.Read(path);
        }

        public void WriteScroll(string path, string scroll)
        {
            _scrollWriter.Write(path, scroll);
        }
    }
}
