using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Templater.Abstractions;
using Templater.Exceptions;

namespace Templater
{
    public class Templater : ITemplater
    {
        private readonly ILogger _logger;

        public Templater(ILogger<Templater> logger)
        {
            _logger = logger;
        }

        public string CreateScroll(string text, Template template)
        {
            var result = string.Empty;

            var beginCapacity = ComputeCapacity(template.Fill, template.Begin);
            var middleCapacity = ComputeCapacity(template.Fill, template.Middle);
            var endCapacity = ComputeCapacity(template.Fill, template.End);

            if (beginCapacity != middleCapacity || middleCapacity != endCapacity)
            {
                if ((beginCapacity != 0 && middleCapacity != beginCapacity) ||
                    (endCapacity != 0 && middleCapacity != endCapacity))
                {
                    _logger.LogError("Template begin, middle and end fill space must be equal (unless begin and/or end have no fillable space)");
                    throw new TemplaterException("Template parts fill space is not equal");
                }
            }

            if (middleCapacity < 1)
            {
                _logger.LogError("Template middle part must have filling space");
                throw new TemplaterException("Fill space not found in template middle part");
            }

            var charsLeft = text.Length;
            result += CreatePart(template.Fill, template.Blank, text, template.Begin);
            charsLeft -= beginCapacity;
            text = text.Substring(beginCapacity);

            var endText = text.Substring(text.Length - Math.Min(endCapacity, text.Length), Math.Min(endCapacity, text.Length));
            var endResult = string.Empty;
            if (charsLeft > 0)
            {
                endResult += CreatePart(template.Fill, template.Blank, endText, template.End);
                charsLeft -= endCapacity;
            }

            text = text.Substring(0, charsLeft);
            while (charsLeft > 0)
            {
                result += CreatePart(template.Fill, template.Blank, text, template.Middle);
                text = text.Substring(Math.Min(middleCapacity, charsLeft));

                charsLeft -= middleCapacity;
            }

            result += endResult;
            return result;
        }

        private int ComputeCapacity(char fill, string part)
        {
            return part.Count(c => c == fill);
        }

        private string CreatePart(char fill, char blank, string text, string templatePart)
        {
            var result = string.Empty;
            var current = 0;

            foreach (var c in templatePart)
            {
                if (c != fill)
                {
                    result += c;
                    continue;
                }

                if (current >= text.Length)
                    result += blank;
                else
                    result += text[current++];

            }

            return result;
        }

    }
}
