using Justifier;

namespace TheElvenScrolls
{
    public class AppSettings
    {
        public string TemplatePath { get; set; }
        public string InputPath { get; set; }
        public string OutputPath { get; set; }

        public TemplateFileSettings TemplateFileSettings { get; set; }

        public JustifierSettings JustifierSettings { get; set; }
    }
}
