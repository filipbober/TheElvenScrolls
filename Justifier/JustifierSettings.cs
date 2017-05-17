using System.Collections.Generic;

namespace Justifier
{
    public class JustifierSettings
    {
        public double EndingThresholdPercent { get; set; } = 0.9;
        public bool PauseAfterLongWords { get; set; } = true;
        public bool IndentParagraphs { get; set; } = true;
        public bool BreakOnlyOnEmptyLines { get; set; } = true;

        public string Paragraph { get; set; } = "   ";
        public string Pause { get; set; } = "-";
        public HashSet<char> ExcludedPunctuations { get; set; }
    }
}
