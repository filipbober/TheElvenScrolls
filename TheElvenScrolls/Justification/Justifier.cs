﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TheElvenScrolls.Justification
{
    class Justifier
    {
        // TODO: add newline as '\n' or '\n\r'
        // TODO: indent at the beginning of paragraph
        // TODO: Consider case when word is longer than line

        private const char NewLine = '\n';
        private const char Space = ' ';
        private const string DoubleSpace = "  ";

        private readonly double _endingThresholdPercent;

        private double _justifyLongerThan = 0;

        private IList<TextChunk> _textChunks;

        public Justifier(double endingThresholdPercent = 0.9)
        {
            _endingThresholdPercent = endingThresholdPercent;
        }

        public string Justify(string text, int width)
        {
            SetLastLineWidth(width);

            string result = string.Empty;

            text = RemoveMultipleSpaces(text);

            _textChunks = CreateFragmentedText(text);

            var lines = CreateJustifiedLines(width, _textChunks);

            result += " ________________________________\n";
            foreach (var line in lines)
                result += "| " + line + " |\n";
            result += " ________________________________\n";

            return result;
        }

        private void SetLastLineWidth(int width)
        {
            bool isInvalid = _endingThresholdPercent > 1.0 || _endingThresholdPercent < 0.0 || _endingThresholdPercent == 0.0;

            if (isInvalid)
            {
                // TODO: Log warn
                _justifyLongerThan = 0;
                return;
            }

            _justifyLongerThan = width * _endingThresholdPercent;
        }

        private string RemoveMultipleSpaces(string text)
        {
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

        private IList<TextChunk> CreateFragmentedText(string text)
        {
            var result = new List<TextChunk>();

            var paragraphs = new List<string>();
            foreach (var paragraph in text.Split(NewLine))
            {
                paragraphs.Add(paragraph);
            }

            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrEmpty(paragraph))
                {
                    result.Add(new TextChunk(NewLine, ChunkType.NewLine));
                    continue;
                }

                foreach (var word in paragraph.Split(Space))
                {
                    result.Add(new TextChunk(word, ChunkType.Word));

                    if (char.IsPunctuation(word.Substring(word.Length - 1)[0]))
                        result.Add(new TextChunk(DoubleSpace, ChunkType.Space));
                    else
                        result.Add(new TextChunk(Space, ChunkType.Space));
                }
            }

            return result;
        }

        private IList<string> CreateJustifiedLines(int width, IList<TextChunk> chunks)
        {
            // TODO: Add case when word is longer than line

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
                    // justify line
                    //newLine.Add(chunk);
                    //lines.Add(JustifyLine(newLine, currentWidth, width));
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

                    // justify line
                    //currentWidth += chunk.Text.Length;
                    lines.Add(JustifyLine(newLine, currentWidth, width));
                    currentWidth = 0;
                    newLine = new List<TextChunk>();
                }

                // word fits line
                if (currentWidth + chunk.Text.Length <= width)
                {
                    // TODO: Add case when there is '\n' forcing newline

                    newLine.Add(chunk);
                    currentWidth += chunk.Text.Length;
                }
                else
                {
                    // TODO: Error
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

            // TODO: Find better way to check if new line
            var lineEndIdx = lineChunks.Count - 1;
            while (lineEndIdx > 0 && lineChunks[lineEndIdx].Type == ChunkType.NewLine)
                lineEndIdx--;

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

        private string EndLine(string line, int current, int width)
        {
            var result = line;
            for (int i = current + 1; i <= width; i++)
                result += Space;

            return result;
        }
    }
}
