using System;
using System.Collections.Generic;
using System.Text;

namespace TheElvenScrolls
{
    public interface IScrollWriter
    {
        void WriteOutput(string path, string scroll);
    }
}
