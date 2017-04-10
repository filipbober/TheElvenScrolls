﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Justifier.Exceptions;

namespace Justifier
{
    public class Justifier : IJustifier
    {
        private const char NewLine = '\n';
        private const char Space = ' ';
        private const string DoubleSpace = "  ";

        private readonly ILogger<Justifier> _logger;
        private readonly JustifierSettings _settings;

        private double _justifyLongerThan = 0;

        public Justifier(ILoggerFactory loggerFactory, JustifierSettings settings)
        {
            _logger = loggerFactory.CreateLogger<Justifier>();
            _settings = settings;
        }

        public string Justify(string text, int width)
        {
            _logger.LogInformation("Starting justification...");
            SetLastLineWidth(width);

            if (_settings.Paragraph.Length >= width / 2)
            {
                _logger.LogError("Paragraph must be less than half the line width");
                throw new JustifierException("Invalid paragraph");
            }

            string result = string.Empty;
            text = RemoveMultipleSpaces(text);
            var textChunks = CreateFragmentedText(text, width);
            var lines = CreateJustifiedLines(width, textChunks);

            result += " ________________________________\n";
            foreach (var line in lines)
                result += "| " + line + " |\n";
            result += " ________________________________\n";

            return result;
        }

        private void SetLastLineWidth(int width)
        {
            bool isInvalid = _settings.EndingThresholdPercent > 1.0 || _settings.EndingThresholdPercent < 0.0 || _settings.EndingThresholdPercent == 0.0;

            if (isInvalid)
            {
                _logger.LogError("Threshold percent must be withing 0.0 and 1.0");
                throw new JustifierException("Invalid line ending threshold percent");
            }

            _justifyLongerThan = width * _settings.EndingThresholdPercent;
        }

        private string RemoveMultipleSpaces(string text)
        {
            _logger.LogDebug("Removing excess spaces");

            var tmp = Regex.Replace(text, @"[^\S\r\n]+", " ");

            string result = string.Empty;
            var lines = tmp.Split(NewLine);
            foreach (var line in lines)
            {
                result += line.Trim();
                result += NewLine;
            }

            return result;
        }

        private IList<TextChunk> CreateFragmentedText(string text, int width)
        {
            _logger.LogDebug("Creating fragmented text");

            var result = new List<TextChunk>();

            var paragraphs = new List<string>();
            foreach (var paragraph in text.Split(NewLine))
            {
                paragraphs.Add(paragraph);
            }

            var firstWord = true;
            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrEmpty(paragraph))
                {
                    result.Add(new TextChunk(NewLine, ChunkType.NewLine));

                    if (_settings.IndentParagraphs)
                        result.Add(new TextChunk(_settings.Paragraph, ChunkType.Paragraph));

                    firstWord = true;
                    continue;
                }

                foreach (var word in paragraph.Split(Space))
                {                   
                    if (word.Length > width)
                    {
                        var lineWidth = width;
                        if (_settings.IndentParagraphs && firstWord)
                            lineWidth -= _settings.Paragraph.Length;

                        var currentIdx = 0;
                        var lettersLeft = word.Length;
                        while (lettersLeft > 0)
                        {
                            string shorterWord = string.Empty;
                            if (_settings.PauseAfterLongWords && lettersLeft > lineWidth)
                                shorterWord = word.Substring(currentIdx, lineWidth - _settings.Pause.Length) + _settings.Pause;
                            else
                                shorterWord = word.Substring(currentIdx, Math.Min(lineWidth, lettersLeft));

                            result.Add(new TextChunk(shorterWord, ChunkType.Word));
                            lettersLeft -= shorterWord.Length;
                            currentIdx += shorterWord.Length;

                            firstWord = false;
                            lineWidth += _settings.Paragraph.Length;
                        }
                    }
                    else
                        result.Add(new TextChunk(word, ChunkType.Word));

                    if (char.IsPunctuation(word.Substring(word.Length - 1)[0]))
                        result.Add(new TextChunk(DoubleSpace, ChunkType.Space));
                    else
                        result.Add(new TextChunk(Space, ChunkType.Space));

                    firstWord = false;
                }
            }

            return result;
        }

        private IList<string> CreateJustifiedLines(int width, IList<TextChunk> chunks)
        {
            _logger.LogDebug("Creating justified lines");

            var lines = new List<string>();

            var newLine = new List<TextChunk>();

            var currentWidth = 0;
            foreach (var chunk in chunks)
            {
                if (currentWidth == 0)
                {
                    if (chunk.Type == ChunkType.Space)
                        continue;
                }

                if (chunk.Type == ChunkType.NewLine)
                {
                    lines.Add(JustifyLastLine(newLine, currentWidth, width));
                    lines.Add(AddBlankLine(width));
                    currentWidth = 0;
                    newLine = new List<TextChunk>();
                    continue;
                }

                if (currentWidth + chunk.Text.Length > width)
                {
                    // Skip spaces at the beginning of the line
                    if (chunk.Type == ChunkType.Space)
                        continue;

                    lines.Add(JustifyLine(newLine, currentWidth, width));
                    currentWidth = 0;
                    newLine = new List<TextChunk>();
                }

                if (currentWidth + chunk.Text.Length <= width)
                {
                    newLine.Add(chunk);
                    currentWidth += chunk.Text.Length;
                }
                else
                {
                    _logger.LogError("Chunk cannot be processed: {0}", chunk.Text);                    
                    continue;
                }
            }

            return lines;
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

            var spaceChunks = new List<TextChunk>();
            foreach (var chunk in lineChunks)
            {
                if (chunk.Type == ChunkType.Space)
                    spaceChunks.Add(chunk);
            }

            if (spaceChunks.Count > 0)
            {
                Random rnd = new Random();
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

                _logger.LogDebug("Long line: {0}", result);
            }

            return result;
        }

        private string JustifyLastLine(IList<TextChunk> lineChunks, int lineWidth, int width)
        {
            var result = string.Empty;

            if (lineWidth < _justifyLongerThan)
            {
                foreach (var chunk in lineChunks)
                {
                    result += chunk.Text;
                }

                while (result.Length < width)
                    result += Space;

                return result;
            }
            else
            {
                return JustifyLine(lineChunks, lineWidth, width);
            }
        }

        private string AddBlankLine(int width)
        {
            var result = string.Empty;

            for (int i = 0; i < width; i++)
                result += Space;

            return result;
        }
    }
}