using System;

namespace TheElvenScrolls.Events
{
    internal sealed class PathChangedArgs : EventArgs
    {
        public string Path { get; }

        public PathChangedArgs(string path)
        {
            Path = path;
        }

    }
}
