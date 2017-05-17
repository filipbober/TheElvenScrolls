using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using TheElvenScrolls.Events;
using TheElvenScrolls.Menu.Abstractions;

namespace TheElvenScrolls.Menu
{
    internal sealed class SpecifyOutputCommand : IMenuCommand
    {
        public string Description => "Specify output file";

        internal event EventHandler<PathChangedArgs> OutputChanged = delegate { };

        private readonly ILogger _logger;
        private readonly IMenu _menu;
        private readonly string _outputDir;

        public SpecifyOutputCommand(ILogger<SpecifyOutputCommand> logger, IMenu menu, string outputDir)
        {
            _logger = logger;
            _menu = menu;
            _outputDir = outputDir;
        }

        public void Execute()
        {
            try
            {
                if (!Directory.Exists(_outputDir))
                {
                    _logger.LogWarning("Directory does not exist. Creating.");
                    Directory.CreateDirectory(_outputDir);
                }
            }
            catch (Exception)
            {
                _logger.LogError("Output directory is invalid");
                throw;
            }

            _menu.Run(CreateMenu(), TryExecute, ExecuteDefault);
        }

        private void OnOutputChanged(PathChangedArgs e)
        {
            OutputChanged?.Invoke(this, e);
        }

        private IList<string> CreateMenu()
        {
            var menu = new List<string> { "Output Menu" };

            var files = Directory.GetFiles(_outputDir).ToList();
            menu.AddRange(files.Select((t, i) => (i + 1) + ". " + Path.GetFileName(t)));

            return menu;
        }

        private bool TryExecute(string option)
        {
            var files = Directory.GetFiles(_outputDir).ToList();

            if (!int.TryParse(option, out var idx) || (idx - 1 >= files.Count || idx < 0 || idx <= 0))
            {
                var filename = option;
                if (string.IsNullOrEmpty(Path.GetExtension(filename)))
                    filename += ".txt";

                var path = Path.Combine(_outputDir, filename);
                if (path.IndexOfAny(Path.GetInvalidPathChars()) > 0 ||
                    Path.GetFileName(path).IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
                {
                    _logger.LogWarning("Invalid path of file name");
                    return false;
                }

                OnOutputChanged(new PathChangedArgs(path));
                _menu.WriteLine("Selected " + Path.GetFileName(path));
                _menu.Wait();

                return true;
            }

            OnOutputChanged(new PathChangedArgs(files[idx - 1]));
            _menu.WriteLine("Selected " + Path.GetFileName(files[idx - 1]));
            _menu.Wait();

            return true;
        }

        private void ExecuteDefault()
        {
            var files = Directory.GetFiles(_outputDir).ToList();
            if (files.Count < 1)
            {
                _logger.LogWarning("There are no files in output directory. Output will remain unchanged");
            }
            else
            {
                OnOutputChanged(new PathChangedArgs(files[0]));
                _menu.WriteLine("Selected " + Path.GetFileName(files[0]));
            }

            _menu.Wait();
        }
    }
}
