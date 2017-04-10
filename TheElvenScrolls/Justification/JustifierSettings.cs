namespace TheElvenScrolls.Justification
{
    class JustifierSettings
    {
        public double EndingThresholdPercent { get; set; } = 0.9;
        public bool PauseAfterLongWords { get; set; } = true;
        public bool IndentParagraphs { get; set; } = true;

        public string Paragraph { get; set; } = "   ";
        public string Pause { get; set; } = "-";

        public JustifierSettings(double endingThreshold, bool pauseAfterLongWords, bool indent, string paragraph, string pause)
            : this(endingThreshold, pauseAfterLongWords, indent)
        {
            Paragraph = paragraph;
            Pause = pause;
        }

        public JustifierSettings(double endingThreshold, bool pauseAfterLongWords, bool indent)
        {
            EndingThresholdPercent = endingThreshold;
            PauseAfterLongWords = pauseAfterLongWords;
            IndentParagraphs = indent;
        }

        public JustifierSettings()
        {
        }
    }
}
