using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Templater.Abstractions;
using Templater.Exceptions;

namespace Templater
{
    public sealed class Templater : ITemplater
    {
        private readonly ILogger _logger;

        private int _currentLineIdx;

        public Templater(ILogger<Templater> logger)
        {
            _logger = logger;
        }

        public string Create(IList<string> textLines, Template template)
        {
            var result = string.Empty;

            if (textLines == null)
                return result;

            var beginLines = Regex.Split(template.Begin, "\r\n|\r|\n").ToList();
            beginLines.RemoveAt(beginLines.Count - 1);

            var middleLines = Regex.Split(template.Middle, "\r\n|\r|\n").ToList();
            middleLines.RemoveAt(middleLines.Count - 1);

            var endLines = Regex.Split(template.End, "\r\n|\r|\n").ToList();
            endLines.RemoveAt(endLines.Count - 1);

            _currentLineIdx = 0;
            var currentTemplateIdx = 0;

            var beginReady = false;
            var endReady = false;

            var endResult = string.Empty;
            var endFillableLinesCount = CountFillableLines(endLines, template.Fill);
            while (_currentLineIdx < textLines.Count)
            {
                string textLine;

                if (!beginReady && currentTemplateIdx < beginLines.Count)
                {
                    foreach (var templateLine in beginLines)
                    {
                        textLine = _currentLineIdx >= textLines.Count ? string.Empty : textLines[_currentLineIdx];
                        result += CreateLine(template.Fill, template.Blank, textLine, templateLine);
                    }

                    currentTemplateIdx += beginLines.Count;
                    beginReady = true;
                }

                if (!endReady && textLines.Count - _currentLineIdx <= endFillableLinesCount)
                {
                    foreach (var templateLine in endLines)
                    {
                        textLine = _currentLineIdx >= textLines.Count ? string.Empty : textLines[_currentLineIdx];
                        endResult += CreateLine(template.Fill, template.Blank, textLine, templateLine);
                    }

                    currentTemplateIdx += endLines.Count;
                    endReady = true;
                }
                else
                {
                    textLine = textLines[_currentLineIdx];
                    var middleLineIdx = currentTemplateIdx % middleLines.Count;
                    var templateLine = middleLines[middleLineIdx];
                    result += CreateLine(template.Fill, template.Blank, textLine, templateLine);
                    currentTemplateIdx++;
                }
            }

            if (!endReady)
            {
                foreach (var templateLine in endLines)
                {
                    result += CreateLine(template.Fill, template.Blank, string.Empty, templateLine);
                }
            }

            result += endResult;
            return result;
        }

        private string CreateLine(char fill, char blank, string textLine, string templateLine)
        {
            if (!string.IsNullOrEmpty(textLine) &&
                templateLine.Contains(fill) &&
                templateLine.Count(c => c == fill) != textLine.Length)
                _logger.LogWarning("Line does not fit template, possible formatting errors: {0}", textLine);

            var result = CreatePart(fill, blank, textLine, templateLine + "\n");

            if (!string.IsNullOrEmpty(textLine) && templateLine.Contains(fill))
                _currentLineIdx++;

            return result;
        }

        private static int CountFillableLines(IList<string> templateLines, char fill)
        {
            return templateLines.Count(line => line.Count(c => c == fill) > 0);
        }

        private static string CreatePart(char fill, char blank, string text, string templateLine)
        {
            if (text == null)
                throw new TemplaterException("Text line is null");

            var result = string.Empty;
            var current = 0;

            foreach (var c in templateLine)
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
