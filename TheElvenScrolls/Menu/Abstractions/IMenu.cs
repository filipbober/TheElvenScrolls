using System;
using System.Collections.Generic;

namespace TheElvenScrolls.Menu.Abstractions
{
    public interface IMenu
    {
        void Run(IList<string> menu, Func<string, bool> tryExecute, Action executeDefault);
        void Wait();
        void Write(string text);
        void NewLine();
        void WriteLine(string line);
        void WriteLine(string format, params object[] arg);
    }
}
