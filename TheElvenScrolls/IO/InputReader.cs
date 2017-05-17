using System.IO;
using Microsoft.Extensions.Logging;
using TheElvenScrolls.IO.Abstractions;

namespace TheElvenScrolls.IO
{
    internal sealed class InputReader : IInputReader
    {
        private readonly ILogger _logger;

        public InputReader(ILogger<InputReader> logger)
        {
            _logger = logger;
        }

        public string Read(string path)
        {
            if (!File.Exists(path))
            {
                _logger.LogError("Specified path is not a valid input text file");
                throw new FileNotFoundException("Input file not found");
            }

            var result = string.Empty;

            var fileStream = new FileStream(path, FileMode.Open);
            using (var reader = new StreamReader(fileStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Replace("\r", "");
                    result += line + "\n";
                }
            }

            return result;
        }
    }
}
