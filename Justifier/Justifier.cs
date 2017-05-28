using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Justifier.Abstractions;
using Justifier.Exceptions;

namespace Justifier
{
    public sealed class Justifier : JustifierBase, IJustifier, IVariableWidthJustifier
    {
        private readonly ILogger _logger;

        private bool _nextNewLine;
        private string _lastLeft;

        public Justifier(ILogger<Justifier> logger, JustifierSettings settings) : base(logger, settings)
        {
            _logger = logger;
        }

        public IList<string> Justify(string text, int width)
        {
            _logger.LogInformation("Justifying");

            if (text.Length < 1)
            {
                _logger.LogWarning("Text is empty");
                return new List<string>();
            }

            if (Settings.Paragraph.Length >= width / 2)
            {
                _logger.LogError("Paragraph must be less than half the line width");
                throw new JustifierException("Invalid paragraph");
            }

            SetJustifyWidthThreshold(width);
            SetupChunks(text);

            return CreateJustifiedLines(width);
        }

        public string JustifyNextLine(ref string left, int width)
        {
            SetJustifyWidthThreshold(width);

            if (_nextNewLine)
            {
                _nextNewLine = false;
                return AddBlankLine(width);
            }

            var isNewText = _lastLeft == null || _lastLeft != left;
            if (isNewText)
                SetupChunks(left);

            left = string.Empty;

            ResetLines();
            ResetNewLine();

            bool isLast;
            while (!(isLast = !ProcessCurrentChunk(width)))
            {
                if (Lines.Count > 0)
                    break;

                CurrentChunk++;
            }

            left += GetNewLine();
            left += GetUnprocessedText();

            _lastLeft = left;
            if (isLast)
            {
                left = string.Empty;

                return Lines.FirstOrDefault();
            }

            if (Lines.Count > 1 && string.IsNullOrWhiteSpace(Lines[1]))
                _nextNewLine = true;

            return Lines.FirstOrDefault();
        }

        private IList<string> CreateJustifiedLines(int width)
        {
            _logger.LogDebug("Creating justified lines");

            ResetLines();
            ResetNewLine();

            while (ProcessCurrentChunk(width))
                CurrentChunk++;


            return Lines;
        }
    }
}
