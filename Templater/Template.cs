namespace Templater
{
    public class Template
    {
        public char Fill { get; set; } = '+';
        public char Blank { get; set; } = ' ';

        public string Begin { get; set; } = "++++++++++++++++++++++++++++++++\n";
        public string Middle { get; set; } = "++++++++++++++++++++++++++++++++\n";
        public string End { get; set; } = "++++++++++++++++++++++++++++++++\n";
    }
}