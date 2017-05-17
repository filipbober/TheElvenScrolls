using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using TheElvenScrolls.Events;
using TheElvenScrolls.Menu.Abstractions;

namespace TheElvenScrolls.Menu
{
    internal sealed class ChooseInputCommand : IMenuCommand
    {
        public string Description => "Choose input file";

        internal event EventHandler<PathChangedArgs> InputChanged = delegate { };

        private readonly ILogger _logger;
        private readonly IMenu _menu;
        private readonly string _inputDir;

        public ChooseInputCommand(ILogger<ChooseInputCommand> logger, IMenu menu, string inputDir)
        {
            _logger = logger;
            _menu = menu;
            _inputDir = inputDir;
        }

        public void Execute()
        {
            try
            {
                if (!Directory.Exists(_inputDir))
                {
                    _logger.LogWarning("Directory does not exist. Creating");
                    Directory.CreateDirectory(_inputDir);
                }
            }
            catch (Exception)
            {
                _logger.LogError("Input directory is invalid");
                throw;
            }

            _menu.Run(CreateMenu(), TryExecute, ExecuteDefault);
        }

        private void OnInputChanged(PathChangedArgs e)
        {
            InputChanged?.Invoke(this, e);
        }

        private IList<string> CreateMenu()
        {
            var menu = new List<string> { "Input Menu" };

            var files = Directory.GetFiles(_inputDir);
            menu.AddRange(files.Select((t, i) => (i + 1) + ". " + Path.GetFileName(t)));

            return menu;
        }

        private bool TryExecute(string option)
        {
            var files = Directory.GetFiles(_inputDir).ToList();

            if (!int.TryParse(option, out var idx) || (idx - 1 >= files.Count || idx < 0 || idx <= 0))
                return false;

            OnInputChanged(new PathChangedArgs(files[idx - 1]));
            _menu.WriteLine("Selected " + Path.GetFileName(files[idx - 1]));
            _menu.Wait();

            return true;
        }

        private void ExecuteDefault()
        {
            var files = Directory.GetFiles(_inputDir).ToList();
            if (files.Count < 1)
            {
                _logger.LogWarning("There are no files in input directory. Input will remain unchanged");
            }
            else
            {
                OnInputChanged(new PathChangedArgs(files[0]));
                _menu.WriteLine("Selected " + Path.GetFileName(files[0]));
            }

            _menu.Wait();
        }
    }
}
