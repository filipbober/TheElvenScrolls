using Justifier;

namespace TheElvenScrolls
{
    public class AppSettings
    {
        public string TemplatePath { get; set; }
        public string TextPath { get; set; }
        public string OutputPath { get; set; }

        public JustifierSettings JustifierSettings { get; set; }
    }
}
