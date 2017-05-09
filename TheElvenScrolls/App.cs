using Justifier;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Templater;

namespace TheElvenScrolls
{
    public class App
    {
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        private readonly IInputReader _inputReader;
        private readonly ITemplateReader _templateReader;
        private readonly IJustifier _justifier;
        private readonly ITemplater _templater;
        private readonly IScrollWriter _scrollWriter;

        public App(ILogger<App> logger, IOptionsSnapshot<AppSettings> settings, IInputReader inputReader, ITemplateReader templateReader, IScrollWriter scrollWriter, IJustifier justifier, ITemplater templater)
        {
            _logger = logger;

            _settings = settings.Value;

            _inputReader = inputReader;
            _templateReader = templateReader;
            _justifier = justifier;
            _templater = templater;
            _scrollWriter = scrollWriter;
        }

        public void Run()
        {
            Console.WriteLine("Copyright (C) 2017 Filip Cyrus Bober");
            Console.WriteLine("The Elven Scrolls ASCII letter generator");

            _logger.LogInformation("Reading text file");
            if (_settings.InputPath == _settings.OutputPath)
            {
                _logger.LogWarning("Replacing intput file with output scroll");
            }
            var text = _inputReader.Read(_settings.InputPath);

            Console.WriteLine("Raw text:");
            Console.WriteLine(text);

            Console.WriteLine("Reading template file");
            var template = _templateReader.ReadTemplate(_settings.TemplatePath);
            var lineWidth = template.Begin.Count(c => c == template.Fill);

            var justified = _justifier.Justify(text, lineWidth);
            _logger.LogInformation("Justification finished");
            Console.Write(justified);

            var scroll = _templater.CreateScroll(justified, template);

            _logger.LogInformation("Scroll ready");
            Console.Write(scroll);

            _scrollWriter.WriteOutput(_settings.OutputPath, scroll);
        }

    }
}
