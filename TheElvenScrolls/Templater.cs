using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TheElvenScrolls
{
    internal class Templater
    {
        private readonly ILogger<Templater> _logger;

        private readonly Template _template;

        public Templater(ILoggerFactory loggerFactory, Template template)
        {
            _logger = loggerFactory.CreateLogger<Templater>();
            _template = template;
        }

        // TODO: Remove
        public Templater(Template template)
        {
            _template = template;
        }

        public string CreateScroll(string text)
        {
            var result = string.Empty;

            var beginCapacity = ComputeCapacity(_template.Begin);
            var middleCapacity = ComputeCapacity(_template.Middle);
            var endCapacity = ComputeCapacity(_template.End);

            if (middleCapacity < 1)
            {
                _logger.LogError("Template body must have filling space");
                // TODO: throw exception
                return result;
            }

            var charsLeft = text.Length;
            result += CreatePart(text, _template.Begin);
            charsLeft -= beginCapacity;
            text = text.Substring(beginCapacity);

            var endText = text.Substring(text.Length - Math.Min(endCapacity, text.Length), Math.Min(endCapacity, text.Length));
            var endResult = string.Empty;
            if (charsLeft > 0)
            {
                endResult += CreatePart(endText, _template.End);
                charsLeft -= endCapacity;
            }

            text = text.Substring(0, charsLeft);
            while (charsLeft > 0)
            {
                result += CreatePart(text, _template.Middle);
                text = text.Substring(Math.Min(middleCapacity, charsLeft));

                charsLeft -= middleCapacity;
            }

            result += endResult;
            return result;
        }

        private int ComputeCapacity(string part)
        {
            return part.Count(c => c == _template.Fill);
        }

        private string CreatePart(string text, string templatePart)
        {
            var result = string.Empty;
            var current = 0;

            foreach (var c in templatePart)
            {
                if (c != _template.Fill)
                {
                    result += c;
                    continue;
                }

                if (current >= text.Length)
                    result += _template.Blank;
                else
                    result += text[current++];

            }

            return result;
        }

    }
}