using Justifier;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Templater;
using Templater.Exceptions;

namespace TheElvenScrolls
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly AppSettings _settings;
        private readonly IJustifier _justifier;
        private readonly ITemplater _templater;

        public App(ILogger<App> logger, IOptionsSnapshot<AppSettings> settings, IJustifier justifier, ITemplater templater)
        {
            _logger = logger;
            _settings = settings.Value;
            _justifier = justifier;
            _templater = templater;
        }

        public void Run()
        {
            Console.WriteLine("Copyright (C) 2017 Filip Cyrus Bober");
            Console.WriteLine("The Elven Scrolls ASCII letter generator");

            _logger.LogInformation("Reading text file");
            var text = ReadInput(_settings.InputPath);

            Console.WriteLine("Raw text:");
            Console.WriteLine(text);

            Console.WriteLine("Reading template file");
            var template = ReadTemplate(_settings.TemplatePath);
            var lineWidth = template.Begin.Count(c => c == template.Fill);

            var justified = _justifier.Justify(text, lineWidth);
            _logger.LogInformation("Justification finished");
            Console.Write(justified);

            var scroll = _templater.CreateScroll(justified, template);

            _logger.LogInformation("Scroll ready");
            Console.Write(scroll);

            WriteOutput(_settings.OutputPath, scroll);
            // ---
        }

        private string ReadInput(string path)
        {
            if (!File.Exists(path))
            {
                _logger.LogError("Specified path is not a valid input text file");
                throw new FileNotFoundException("Input file not found");
            }

            if (_settings.InputPath == _settings.OutputPath)
            {
                _logger.LogWarning("Replacing intput file with output scroll");
                // TODO: check if file not in use
                // TODO: Allow writing to the same file as output.
            }

            var result = string.Empty;

            var fileStream = new FileStream(path, FileMode.Open);
            using (var reader = new StreamReader(fileStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Replace("\r", "");
                    result += line + "\n";
                }
            }

            return result;
        }

        private Template ReadTemplate(string path)
        {
            if (!File.Exists(path))
            {
                _logger.LogError("Specified path is not a valid template file");
                throw new FileNotFoundException("Template not found");
            }

            var template = new Template();

            var fileStream = new FileStream(path, FileMode.Open);
            using (var reader = new StreamReader(fileStream))
            {
                var fillConfigPattern = _settings.TemplateFileSettings.FillConfigPattern;
                var blankConfigPattern = _settings.TemplateFileSettings.BlankConfigPattern;

                string line;
                while ((line = reader.ReadLine()) != null && !SeparatorEncountered(line))
                {
                    if (line.StartsWith(fillConfigPattern))
                        template.Fill = line.Substring(fillConfigPattern.Length, 1)[0];
                    else if (line.StartsWith(blankConfigPattern))
                        template.Blank = line.Substring(blankConfigPattern.Length, 1)[0];
                }

                template.Begin = string.Empty;
                while ((line = reader.ReadLine()) != null && !SeparatorEncountered(line))
                {
                    template.Begin += line + "\n";
                }

                template.Middle = string.Empty;
                while ((line = reader.ReadLine()) != null && !SeparatorEncountered(line))
                {
                    template.Middle += line + "\n";
                }

                template.End = string.Empty;
                while ((line = reader.ReadLine()) != null && !SeparatorEncountered(line))
                {
                    template.End += line + "\n";
                }

            }

            ValidateTemplate(template);

            return template;
        }

        private bool SeparatorEncountered(string line)
        {
            var separator = _settings.TemplateFileSettings.PartSeparatorPattern;

            if (line == separator)
                return true;

            return line.Length == separator.Length && line.StartsWith(separator);
        }

        private void ValidateTemplate(Template template)
        {
            var valid = true;

            if (template.Fill == '\0')
            {
                _logger.LogError("Template Fill must be specified in first or second line");
                valid = false;
            }

            if (template.Blank == '\0')
            {
                _logger.LogError("Template Blank must be specified in first or second line");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(template.Begin))
            {
                _logger.LogError("Template Begin must be specified after config and separated by new line");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(template.Middle))
            {
                _logger.LogError("Template Middle must be specified after Begin and separated by new line");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(template.End))
            {
                _logger.LogError("Template End must be specified after End and separated by new line");
                valid = false;
            }

            if (!valid)
                throw new TemplaterException("Invalid template");
        }

        private void WriteOutput(string path, string scroll)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (isWindows)
                scroll = scroll.Replace("\n", "\r\n");

            File.WriteAllText(path, scroll);
        }

    }
}
