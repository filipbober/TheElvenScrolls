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

        public int ComputePartMaxWidth(string part)
        {
            var width = 0;
            var maxWidth = 0;
            foreach (var c in part)
            {
                if (c == Fill)
                {
                    width++;
                }
                else if (c == '\n')
                {
                    if (maxWidth <= width)
                        maxWidth = width;

                    width = 0;
                }
            }

            return maxWidth;
        }

        public int ComputeLineWidth(string line)
        {
            var width = 0;
            foreach (var c in line)
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
