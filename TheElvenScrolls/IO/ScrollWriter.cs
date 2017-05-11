﻿using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace TheElvenScrolls.IO
{
    internal class ScrollWriter : IScrollWriter
    {
        private readonly ILogger _logger;

        public ScrollWriter(ILogger<ScrollWriter> logger)
        {
            _logger = logger;
        }

        public void WriteOutput(string path, string scroll)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (isWindows)
            {
                _logger.LogDebug("Windows detected. Replacing line endings");
                scroll = scroll.Replace("\n", "\r\n");
            }

            File.WriteAllText(path, scroll);
        }
    }
}
