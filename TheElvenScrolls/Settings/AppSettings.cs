using Justifier;

namespace TheElvenScrolls.Settings
{
    internal sealed class AppSettings
    {
        public DefaultFilesSettings DefaultFilesSettings { get; set; }
        public DirectorySettings DirectorySettings { get; set; }
        public TemplateFileSettings TemplateFileSettings { get; set; }
        public JustifierSettings JustifierSettings { get; set; }
    }
}
