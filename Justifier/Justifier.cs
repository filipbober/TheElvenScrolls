using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Justifier.Abstractions;
using Justifier.Exceptions;

namespace Justifier
{
    public sealed class Justifier : IJustifier
    {
        private const char NewLine = '\n';
        private const char Space = ' ';
        private const string DoubleSpace = "  ";

        private readonly ILogger _logger;
        private readonly JustifierSettings _settings;

        private double _justifyLongerThan;

        private int _currentWidth;
        private List<string> _lines;
        private List<TextChunk> _newLine;

        public Justifier(ILogger<Justifier> logger, JustifierSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public IList<string> Justify(string text, int width)
        {
            _logger.LogInformation("Justifying");

            if (text.Length < 1)
            {
                _logger.LogWarning("Text is empty");
                return new List<string>();
            }

            if (_settings.Paragraph.Length >= width / 2)
            {
                _logger.LogError("Paragraph must be less than half the line width");
                throw new JustifierException("Invalid paragraph");
            }

            SetJustifyWidthThreshold(width);

            text = RemoveMultipleSpaces(text);
            var textChunks = CreateFragmentedText(text);
            var lines = CreateJustifiedLines(width, textChunks);

            return lines;
        }

        private IList<TextChunk> _chunks;
        private int _currentChunk;
        private bool _nextNewLine;

        public string JustifySingleLine(ref string left, int width)
        {
            _settings.EndingThresholdPercent = 0;
            if (_nextNewLine)
            {
                _nextNewLine = false;
                return AddBlankLine(width);
            }

            if (_chunks == null /*|| (_lastLeft == null || _lastLeft != left)*/)
            {
                left = RemoveMultipleSpaces(left);
                _chunks = CreateFragmentedText(left);
                _currentChunk = 0;
            }
            var chunks = _chunks;

            left = string.Empty;
            if (chunks.Count < 1)
            {
                //_lastLeft = null;
                //_chunks = null;
                return string.Empty;
            }

            _lines = new List<string>();
            _newLine = new List<TextChunk>();

            _currentWidth = 0;
            var currentChunk = _currentChunk;
            for (int i = _currentChunk; i < chunks.Count - 1; i++)
            {
                var chunk = chunks[i];
                ProcessChunk(width, chunk);

                if (_lines.Count < 1)
                    currentChunk++;
                else
                    break;


            }

            foreach (var chunk in _newLine)
            {
                left += chunk.Text;
            }

            for (int i = currentChunk; i < chunks.Count; i++)
            {
                //if (chunks[i].Type == ChunkType.NewLine)
                //    left += NewLine;
                //else
                    left += chunks[i].Text;
            }

            _currentChunk = currentChunk;
            //left = RemoveMultipleSpaces(left);
            if (_lines.Count < 1)
            {
                var lastLine = left;
                left = string.Empty;

                AddEndLine(width);
                return _lines[0];

                //return lastLine;
            }

            if (_lines.Count > 1 && string.IsNullOrWhiteSpace(_lines[1]))
                _nextNewLine = true;

            return _lines[0];
        }

        public int PredictLength(string text)
        {
            var normalized = RemoveMultipleSpaces(text);
            var chunks = CreateFragmentedText(normalized);

            return chunks.Where(chunk => chunk.Type != ChunkType.NewLine).Sum(chunk => chunk.Text.Length);
        }

        private void SetJustifyWidthThreshold(int width)
        {
            var isInvalid = _settings.EndingThresholdPercent > 1.0 || _settings.EndingThresholdPercent < 0.0;
            if (isInvalid)
            {
                _logger.LogError("Threshold percent must be within 0.0 and 1.0");
                throw new JustifierException("Invalid line ending threshold percent");
            }

            _justifyLongerThan = width * _settings.EndingThresholdPercent;
        }

        private string RemoveMultipleSpaces(string text)
        {
            _logger.LogDebug("Removing excess spaces");

            var tmp = Regex.Replace(text, @"[^\S\r\n]+", " ");

            var result = string.Empty;
            var lines = tmp.Split(NewLine);
            foreach (var line in lines)
            {
                result += line.Trim();
                result += NewLine;
            }

            return result;
        }

        private IList<TextChunk> CreateFragmentedText(string text)
        {
            _logger.LogDebug("Creating fragmented text");

            var result = new List<TextChunk>();

            var paragraphs = text.Split(NewLine).ToList();

            for (int i = paragraphs.Count - 1; i > 0; i--)
            {
                if (string.IsNullOrEmpty(paragraphs[i]))
                    paragraphs.RemoveAt(i);
                else
                    break;
            }

            if (_settings.IndentParagraphs)
                result.Add(new TextChunk(_settings.Paragraph, ChunkType.Paragraph));

            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrEmpty(paragraph))
                {
                    result.Add(new TextChunk(NewLine, ChunkType.NewLine));

                    if (_settings.IndentParagraphs)
                        result.Add(new TextChunk(_settings.Paragraph, ChunkType.Paragraph));

                    continue;
                }

                if (!_settings.BreakOnlyOnEmptyLines)
                    result.Add(new TextChunk(NewLine, ChunkType.NewLine));


                foreach (var word in paragraph.Split(Space))
                {
                    result.Add(new TextChunk(word, ChunkType.Word));

                    var useDoubleSpace = EndsWithPunctuation(word);
                    result.Add(useDoubleSpace ? new TextChunk(DoubleSpace, ChunkType.Space) : new TextChunk(Space, ChunkType.Space));
                }
            }

            return result;
        }

        private bool EndsWithPunctuation(string word)
        {
            var end = word.Substring(word.Length - 1)[0];

            if (!char.IsPunctuation(end))
                return false;

            return !_settings.ExcludedPunctuations.Contains(end);
        }

        private IList<string> CreateJustifiedLines(int width, IList<TextChunk> chunks)
        {
            _logger.LogDebug("Creating justified lines");

            if (chunks.Count < 1)
                return new List<string>();

            _lines = new List<string>();
            _newLine = new List<TextChunk>();
            //_newLine = new LinkedList<TextChunk>();

            _currentWidth = 0;
            for (int i = 0; i < chunks.Count - 1; i++)
            {
                var chunk = chunks[i];


                ProcessChunk(width, chunk);
            }

            _lines.Add(JustifyLastLine(width));

            return _lines;
        }

        private void ProcessChunk(int width, TextChunk chunk)
        {
            if (_currentWidth == 0)
            {
                if (chunk.Type == ChunkType.Space || chunk.Type == ChunkType.NewLine)
                    return;
            }

            if (chunk.Type == ChunkType.NewLine)
            {
                AddEndLine(width);
                return;
            }

            var wordLength = chunk.Text.Length;
            if (_currentWidth + wordLength > width)
            {
                if (chunk.Type == ChunkType.Space)
                    return;

                if (wordLength < width)
                {
                    AddLine(width);
                }
                else
                {
                    AddLine(width);
                    ProcessLongWord(width, chunk);
                    return;
                }
            }

            if (_currentWidth + chunk.Text.Length <= width)
            {
                _newLine.Add(chunk);
                _currentWidth += chunk.Text.Length;
            }
            else
            {
                _logger.LogError("Chunk cannot be processed: {0}", chunk.Text);
            }
        }

        private void ProcessLongWord(int width, TextChunk chunk)
        {
            var currentIdx = 0;
            var lettersLeft = chunk.Text.Length;
            var word = chunk.Text;
            while (lettersLeft > 0)
            {
                string shorterWord;
                if (_settings.PauseAfterLongWords && lettersLeft > width)
                    shorterWord = word.Substring(currentIdx, width - _settings.Pause.Length) + _settings.Pause;
                else
                    shorterWord = word.Substring(currentIdx, Math.Min(width, lettersLeft));

                _newLine.Add(new TextChunk(shorterWord, chunk.Type));
                AddLine(width);

                lettersLeft -= shorterWord.Length;
                currentIdx += shorterWord.Length;

                if (_currentWidth + chunk.Text.Length > width)
                    continue;

                _newLine.Add(new TextChunk(shorterWord, chunk.Type));
                _currentWidth += shorterWord.Length;
                break;
            }
        }

        private void AddEndLine(int width)
        {
            _lines.Add(JustifyLastLine(width));

            if (_settings.BreakOnlyOnEmptyLines)
                _lines.Add(AddBlankLine(width));

            _currentWidth = 0;
            _newLine = new List<TextChunk>();
        }

        private void AddLine(int width)
        {
            _lines.Add(JustifyLine(_newLine, _currentWidth, width));
            _currentWidth = 0;
            _newLine = new List<TextChunk>();
        }

        private string JustifyLine(IList<TextChunk> lineChunks, int lineWidth, int width)
        {
            var result = string.Empty;
            if (lineChunks == null || lineChunks.Count == 0)
            {
                for (int i = 0; i < width; i++)
                    result += Space;

                return result;
            }

            var lineEndIdx = lineChunks.Count - 1;
            if (lineChunks.Count > 1)
            {
                if (lineChunks[lineEndIdx].Type == ChunkType.Space
                    || lineChunks[0].Type == ChunkType.Space)
                {
                    lineWidth -= lineChunks[lineEndIdx].Text.Length;
                    lineChunks.RemoveAt(lineEndIdx);
                }
            }

            var spaceChunks = lineChunks.Where(chunk => chunk.Type == ChunkType.Space).ToList();
            if (spaceChunks.Count > 0)
            {
                var rnd = new Random();
                while (lineWidth < width)
                {
                    var idx = rnd.Next(spaceChunks.Count);
                    spaceChunks[idx].Text += Space;
                    lineWidth++;
                }

                foreach (var chunk in lineChunks)
                {
                    result += chunk.Text;
                }
            }
            else
            {
                foreach (var chunk in lineChunks)
                    result += chunk.Text;
            }

            while (result.Length < width)
                result += Space;

            _logger.LogDebug("Long line: {0}", result);
            return result;
        }

        private string JustifyLastLine(int width)
        {
            var result = string.Empty;

            if (_currentWidth > _justifyLongerThan)
                return JustifyLine(_newLine, _currentWidth, width);

            foreach (var chunk in _newLine)
                result += chunk.Text;

            while (result.Length < width)
                result += Space;

            return result;
        }

        private static string AddBlankLine(int width)
        {
            var result = string.Empty;

            for (int i = 0; i < width; i++)
                result += Space;

            return result;
        }
    }
}
