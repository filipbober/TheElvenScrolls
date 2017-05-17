using System.Linq;

namespace Templater
{
    public class Template
    {
        public char Fill { get; set; }
        public char Blank { get; set; }

        public string Begin { get; set; }
        public string Middle { get; set; }
        public string End { get; set; }

        public int ComputeCapacity(string part)
        {
            return part.Count(c => c == Fill);
        }

        public int ComputeLineWidth(string part)
        {
            var width = 0;
            foreach (var c in Middle)
            {
                if (c == Fill)
                    width++;
                else if (c == '\n')
                    break;
            }

            return width;
        }
    }
}
