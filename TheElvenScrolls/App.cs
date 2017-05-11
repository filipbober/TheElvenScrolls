using Justifier;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Justifier.Abstractions;
using Templater;
using Templater.Abstractions;
using TheElvenScrolls.IO.Abstractions;
using TheElvenScrolls.Settings;

namespace TheElvenScrolls
{
    internal class App
    {
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        private readonly IInputReader _inputReader;
        private readonly ITemplateReader _templateReader;
        private readonly IJustifier _justifier;
        private readonly ITemplater _templater;
        private readonly IScrollWriter _scrollWriter;

        public App(ILogger<App> logger, AppSettings settings, IInputReader inputReader, ITemplateReader templateReader, IScrollWriter scrollWriter, IJustifier justifier, ITemplater templater)
        {
            _logger = logger;

            _settings = settings;

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
            Console.WriteLine("v" + Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine();

            if (_settings.InputPath == _settings.OutputPath)
            {
                _logger.LogWarning("Replacing intput file with output scroll");
            }

            _logger.LogInformation("Reading input file");
            var text = _inputReader.Read(_settings.InputPath);

            _logger.LogDebug("Raw text:\n" + text);

            _logger.LogDebug("Reading template file");
            var template = _templateReader.ReadTemplate(_settings.TemplatePath);

            var lineWidth = template.Middle.Count(c => c == template.Fill);
            var justified = _justifier.Justify(text, lineWidth);
            _logger.LogInformation("Justification finished");
            _logger.LogDebug("\n" + justified);

            var scroll = _templater.CreateScroll(justified, template);
            _logger.LogInformation("Scroll ready:\n" + scroll);

            _scrollWriter.WriteOutput(_settings.OutputPath, scroll);
            _logger.LogInformation("Scroll saved under path: " + Directory.GetCurrentDirectory() + _settings.OutputPath);

            Console.Write("Press any key to exit...");
        }

    }
}
