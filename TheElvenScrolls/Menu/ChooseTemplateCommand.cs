using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using TheElvenScrolls.Events;
using TheElvenScrolls.Menu.Abstractions;

namespace TheElvenScrolls.Menu
{
    internal sealed class ChooseTemplateCommand : IMenuCommand
    {
        public string Description => "Choose template";

        internal event EventHandler<PathChangedArgs> TemplateChanged = delegate { };

        private readonly ILogger _logger;
        private readonly IMenu _menu;
        private readonly string _templatesDir;

        public ChooseTemplateCommand(ILogger<ChooseTemplateCommand> logger, IMenu menu, string templatesDir)
        {
            _logger = logger;
            _menu = menu;
            _templatesDir = Path.Combine(Directory.GetCurrentDirectory(), templatesDir);
        }

        public void Execute()
        {
            try
            {
                if (!Directory.Exists(_templatesDir))
                {
                    _logger.LogWarning("Directory does not exist. Creating");
                    Directory.CreateDirectory(_templatesDir);
                }
            }
            catch (Exception)
            {
                _logger.LogError("Templates directory is invalid");
                throw;
            }

            _menu.Run(CreateMenu(), TryExecute, ExecuteDefault);
        }

        private void OnTemplateChanged(PathChangedArgs e)
        {
            TemplateChanged?.Invoke(this, e);
        }

        private IList<string> CreateMenu()
        {
            var menu = new List<string> { "Template Menu" };

            var files = Directory.GetFiles(_templatesDir).ToList();
            menu.AddRange(files.Select((t, i) => (i + 1) + ". " + Path.GetFileName(t)));

            return menu;
        }

        private bool TryExecute(string option)
        {
            var files = Directory.GetFiles(_templatesDir).ToList();

            if (!int.TryParse(option, out var idx) || (idx - 1 >= files.Count || idx < 0 || idx <= 0))
                return false;

            OnTemplateChanged(new PathChangedArgs(files[idx - 1]));
            _menu.WriteLine("Selected " + Path.GetFileName(files[idx - 1]));
            _menu.Wait();

            return true;
        }

        private void ExecuteDefault()
        {
            var files = Directory.GetFiles(_templatesDir).ToList();
            if (files.Count < 1)
            {
                _logger.LogWarning("There are no files in templates directory. Template will remain unchanged");
            }
            else
            {
                OnTemplateChanged(new PathChangedArgs(files[0]));
                _menu.WriteLine("Selected " + Path.GetFileName(files[0]));
            }

            _menu.Wait();
        }
    }
}
