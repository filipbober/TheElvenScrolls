using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Justifier.Exceptions;
using Microsoft.Extensions.Logging;

namespace Justifier
{
    public abstract class JustifierBase
    {
        private const char NewLine = '\n';
        private const char Space = ' ';
        private const string DoubleSpace = "  ";

        protected readonly JustifierSettings Settings;

        protected IList<string> Lines { get; private set; } = new List<string>();
        protected int CurrentChunk;

        private readonly ILogger _logger;

        private double _justifyLongerThan;
        private int _currentWidth;
        private IList<TextChunk> _newLine;
        private IList<TextChunk> _chunks;

        protected JustifierBase(ILogger<JustifierBase> logger, JustifierSettings settings)
        {
            _logger = logger;
            Settings = settings;
        }

        public int PredictLength(string text)
        {
            var normalized = RemoveMultipleSpaces(text);
            var chunks = CreateFragmentedText(normalized);

            return chunks.Where(chunk => chunk.Type != ChunkType.NewLine).Sum(chunk => chunk.Text.Length);
        }

        protected void ResetLines()
        {
            Lines = new List<string>();
        }

        protected void ResetNewLine()
        {
            _currentWidth = 0;
            _newLine = new List<TextChunk>();
        }

        protected string GetNewLine()
        {
            return _newLine.Aggregate(string.Empty, (current, chunk) => current + chunk.Text);
        }

        protected string GetUnprocessedText()
        {
            var result = string.Empty;
            for (int i = CurrentChunk; i < _chunks.Count; i++)
                result += _chunks[i].Text;

            return result;
        }

        protected void SetJustifyWidthThreshold(int width)
        {
            var isInvalid = Settings.EndingThresholdPercent > 1.0 || Settings.EndingThresholdPercent < 0.0;
            if (isInvalid)
            {
                _logger.LogError("Threshold percent must be within 0.0 and 1.0");
                throw new JustifierException("Invalid line ending threshold percent");
            }

            _justifyLongerThan = width * Settings.EndingThresholdPercent;
        }

        protected void SetupChunks(string text)
        {
            CurrentChunk = 0;
            _currentWidth = 0;

            var normalized = RemoveMultipleSpaces(text);
            _chunks = CreateFragmentedText(normalized);
        }

        protected bool ProcessCurrentChunk(int width)
        {
            if (CurrentChunk >= _chunks.Count)
            {
                Lines.Add(JustifyLastLine(width));
                return false;
            }

            ProcessChunk(width, _chunks[CurrentChunk]);

            return true;
        }




        // private
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

            if (Settings.IndentParagraphs)
                result.Add(new TextChunk(Settings.Paragraph, ChunkType.Paragraph));

            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrEmpty(paragraph))
                {
                    result.Add(new TextChunk(NewLine, ChunkType.NewLine));

                    if (Settings.IndentParagraphs)
                        result.Add(new TextChunk(Settings.Paragraph, ChunkType.Paragraph));

                    continue;
                }

                if (!Settings.BreakOnlyOnEmptyLines)
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

            return !Settings.ExcludedPunctuations.Contains(end);
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
                if (Settings.PauseAfterLongWords && lettersLeft > width)
                    shorterWord = word.Substring(currentIdx, width - Settings.Pause.Length) + Settings.Pause;
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

        private void AddLine(int width)
        {
            Lines.Add(JustifyLine(_newLine, _currentWidth, width));
            _currentWidth = 0;
            _newLine = new List<TextChunk>();
        }

        protected void AddEndLine(int width)
        {
            Lines.Add(JustifyLastLine(width));

            if (Settings.BreakOnlyOnEmptyLines)
                Lines.Add(AddBlankLine(width));

            _currentWidth = 0;
            _newLine = new List<TextChunk>();
        }

        protected static string AddBlankLine(int width)
        {
            var result = string.Empty;

            for (int i = 0; i < width; i++)
                result += Space;

            return result;
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
    }
}
