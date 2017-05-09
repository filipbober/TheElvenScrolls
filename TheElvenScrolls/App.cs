using Justifier;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
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

            const string text = @"Test justify paragraph

               Test justify paragraph

            Test justify paragraph

            LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong Short Short

            Short Short LongLongLongLongLongLongLongLongLongLongLongLongLong Short Short

            Once         upon a midnight dreary, while I pondered, weak and weary,
            Over many a quaint and curious volume of forgotten lore,
            While I nodded, nearly napping, suddenly there came a tapping,
            As of someone gently rapping, tapping at my chamber door.
            'Tis some visitor, I muttered, tapping at my chamber door-
            Only this, and nothing more.

            Dwa slowa.

            Ah, distinctly I remember it was in a bleak December,
            And each separate dying ember wrought its ghost upon the floor.
            Eagerly I wished the morrow; -vainly I had sought to borrow
            From my books surcease of sorrow - sorrow for the lost Lenore -
              For the rare and radiant maiden whom the angels name Lenore -
            Nameless here for evermore.";

            Console.WriteLine("Raw text:");
            Console.WriteLine(text);

            var justified = _justifier.Justify(text, 60);
            _logger.LogInformation("Justification finished");
            Console.Write(justified);

            var scroll = _templater.CreateScroll(justified);

            _logger.LogInformation("Scroll ready");
            Console.Write(scroll);
            // ---

            Console.WriteLine("Reading template file");
            var template = ReadTemplate(_settings.TemplatePath);
        }

        private Template ReadTemplate(string templatePath)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Specified path is not a valid template file");
                throw new FileNotFoundException("Template not found");
            }

            var template = new Template();

            var fileStream = new FileStream(templatePath, FileMode.Open);
            using (var reader = new StreamReader(fileStream))
            {
                const string fillConfigPattern = "Fill=";
                const string blankConfigPattern = "Blank=";

                string line;
                while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
                {
                    if (line.StartsWith(fillConfigPattern))
                        template.Fill = line.Substring(fillConfigPattern.Length, 1)[0];
                    else if (line.StartsWith(blankConfigPattern))
                        template.Blank = line.Substring(blankConfigPattern.Length, 1)[0];
                }

                // TODO: Check if empty line after config is skipped
                // TODO: Check if new lines are needed

                template.Begin = string.Empty;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    template.Begin += line;
                }

                template.Middle = string.Empty;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    template.Middle += line;
                }

                template.End = string.Empty;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    template.End += line;
                }

            }

            ValidateTemplate(template);

            return template;
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
