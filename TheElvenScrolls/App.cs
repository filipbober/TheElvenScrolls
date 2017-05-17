﻿using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Templater;
using TheElvenScrolls.Events;
using TheElvenScrolls.Globals;
using TheElvenScrolls.Menu;
using TheElvenScrolls.Menu.Abstractions;
using TheElvenScrolls.Settings;
using TheElvenScrolls.Wrappers.Abstractions;

namespace TheElvenScrolls
{
    internal sealed class App
    {
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        private readonly IMenu _menu;
        private readonly IScribe _scribe;
        private readonly IToolbox _toolbox;

        private IList<IMenuCommand> _commands;
        private Template _template;
        private string _input;
        private string _outputPath;

        public App(ILogger<App> logger, AppSettings settings, IMenu menu, IScribe scribe, IToolbox toolbox)
        {
            _logger = logger;
            _settings = settings;
            _menu = menu;
            _scribe = scribe;
            _toolbox = toolbox;
        }

        public void Run()
        {
            _commands = CreateCommands();

            if (_settings.DefaultFilesSettings.Input == _settings.DefaultFilesSettings.Output)
            {
                _logger.LogWarning("Output scroll will replace input text");
            }

            _menu.Run(CreateMenu(), TryExecute, ExecuteDefault);

            SaveSettings();
            _menu.WriteLine("Press any key to exit...");
        }

        private List<IMenuCommand> CreateCommands()
        {
            var commands = new List<IMenuCommand>();

            var chooseTemplate = new ChooseTemplateCommand(ApplicationLogging.CreateLogger<ChooseTemplateCommand>(), _menu, _settings.DirectorySettings.Template);
            chooseTemplate.TemplateChanged += ChangeTemplate;
            commands.Add(chooseTemplate);

            var chooseInput = new ChooseInputCommand(ApplicationLogging.CreateLogger<ChooseInputCommand>(), _menu, _settings.DirectorySettings.Input);
            chooseInput.InputChanged += ChangeInput;
            commands.Add(chooseInput);

            var specifyOutput = new SpecifyOutputCommand(ApplicationLogging.CreateLogger<SpecifyOutputCommand>(), _menu, _settings.DirectorySettings.Output);
            specifyOutput.OutputChanged += ChangeOutput;
            commands.Add(specifyOutput);

            return commands.ToList();
        }

        private IList<string> CreateMenu()
        {
            var menu = new List<string>
            {
                "Copyright (C) 2017 Filip Cyrus Bober",
                "The Elven Scrolls ASCII letter generator",
                "v" + Assembly.GetEntryAssembly().GetName().Version, "",
                "Main Menu"
            };
            menu.AddRange(_commands.Select((t, i) => (i + 1) + ". " + t.Description));

            return menu;
        }

        private bool TryExecute(string option)
        {
            if (!int.TryParse(option, out var idx) || (idx - 1 >= _commands.Count || idx < 0 || idx <= 0))
                return false;

            _commands[idx - 1].Execute();
            return true;
        }

        private void ExecuteDefault()
        {
            if (_input == null)
            {
                _logger.LogInformation("No input specified. Reading default");
               _input = _scribe.ReadInput(_settings.DefaultFilesSettings.Input);
            }

            if (_template == null)
            {
                _logger.LogInformation("No template specified. Reading default");
                _template = _scribe.ReadTemplate(_settings.DefaultFilesSettings.Template);
            }

            var lineWidth = _template.ComputeLineWidth(_template.Middle);
            var justified = _toolbox.Justify(_input, lineWidth);

            _logger.LogInformation("Justification finished");
            _logger.LogDebug("\n" + justified);

            var scroll = _toolbox.CreateTemplate(justified, _template);
            _logger.LogInformation("Scroll ready:\n" + scroll);

            if (_outputPath == null)
            {
                _outputPath = _settings.DefaultFilesSettings.Output;
            }
            _scribe.WriteScroll(_outputPath, scroll);
            _logger.LogInformation("Scroll saved under path: " + Directory.GetCurrentDirectory() + _outputPath);

            _menu.Wait();
        }

        private void ChangeTemplate(object sender, PathChangedArgs e)
        {
            _logger.LogDebug("Template changed to " + e.Path);
            _template = _scribe.ReadTemplate(e.Path);

            _settings.DefaultFilesSettings.Template = Path.Combine(_settings.DirectorySettings.Template, Path.GetFileName(e.Path));
        }

        private void ChangeInput(object sender, PathChangedArgs e)
        {
            _logger.LogDebug("Input changed to " + e.Path);
            _input = _scribe.ReadInput(e.Path);

            _settings.DefaultFilesSettings.Input = Path.Combine(_settings.DirectorySettings.Input, Path.GetFileName(e.Path));
        }

        private void ChangeOutput(object sender, PathChangedArgs e)
        {
            _logger.LogDebug("Output path changed to " + e.Path);
            _outputPath = e.Path;

            _settings.DefaultFilesSettings.Output = Path.Combine(_settings.DirectorySettings.Output, Path.GetFileName(e.Path));
        }

        private void SaveSettings()
        {
            _logger.LogInformation("Saving settings");
            var json = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.user.json"), json);
        }
    }
}
