using System.IO;
using Microsoft.Extensions.Logging;
using Templater;
using Templater.Exceptions;
using TheElvenScrolls.IO.Abstractions;
using TheElvenScrolls.Settings;

namespace TheElvenScrolls.IO
{
    internal class TemplateReader : ITemplateReader
    {
        private readonly ILogger _logger;

        TemplateFileSettings _settings;

        public TemplateReader(ILogger<TemplateReader> logger, TemplateFileSettings settings)
        {
            _logger = logger;

            _settings = settings;
        }

        public Template ReadTemplate(string path)
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
                var fillConfigPattern = _settings.FillConfigPattern;
                var blankConfigPattern = _settings.BlankConfigPattern;

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
            var separator = _settings.PartSeparatorPattern;

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
    }
}
