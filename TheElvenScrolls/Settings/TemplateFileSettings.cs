namespace TheElvenScrolls.Settings
{
    internal sealed class TemplateFileSettings
    {
        public string FillConfigPattern { get; set; } = "Fill=";
        public string BlankConfigPattern { get; set; } = "Blank=";
        public string PartSeparatorPattern { get; set; } = "";
    }
}
