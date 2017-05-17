// Copyright (C) 2017 Filip Cyrus Bober

using System;
using System.Globalization;
using Justifier;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using Justifier.Abstractions;
using Justifier.Exceptions;
using Microsoft.Extensions.Logging;
using Templater.Abstractions;
using Templater.Exceptions;
using TheElvenScrolls.Globals;
using TheElvenScrolls.IO;
using TheElvenScrolls.IO.Abstractions;
using TheElvenScrolls.Menu;
using TheElvenScrolls.Menu.Abstractions;
using TheElvenScrolls.Settings;
using TheElvenScrolls.Wrappers;
using TheElvenScrolls.Wrappers.Abstractions;

namespace TheElvenScrolls
{
    internal class Program
    {
        private static readonly ILogger Logger = ApplicationLogging.CreateLogger<Program>();

        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                serviceProvider.GetService<App>().Run();
            }
            catch (JustifierException ex)
            {
                Logger.LogCritical(-2, ex, "Creating scroll failed due to Justifier exception");
            }
            catch (TemplaterException ex)
            {
                Logger.LogCritical(-3, ex, "Creating scroll failed due to Templater exception");
            }
            catch (FileNotFoundException ex)
            {
                Logger.LogCritical(-4, ex, "File not found");
            }
            catch (Exception ex)
            {
                Logger.LogCritical(-1, ex, "Creating scroll failed due to unexpected exception");
            }

            Console.ReadKey();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.user.json", optional: true)
                .Build();
            services.AddOptions();

            services.AddSingleton(ApplicationLogging.LoggerFactory
                .AddConsole(configuration.GetSection("logging")));
            services.AddLogging();

            services.Configure<AppSettings>(configuration);
            services.AddScoped(cfg => cfg.GetService<IOptionsSnapshot<AppSettings>>().Value);

            services.Configure<JustifierSettings>(configuration.GetSection("justifierSettings"));
            services.AddScoped(cfg => cfg.GetService<IOptionsSnapshot<JustifierSettings>>().Value);

            services.Configure<TemplateFileSettings>(configuration.GetSection("templateFileSettings"));
            services.AddScoped(cfg => cfg.GetService<IOptionsSnapshot<TemplateFileSettings>>().Value);

            services.AddSingleton<IMenu, ConsoleMenu>();

            services.AddTransient<IJustifier, Justifier.Justifier>();
            services.AddTransient<ITemplater, Templater.Templater>();
            services.AddTransient<IInputReader, InputReader>();
            services.AddTransient<ITemplateReader, TemplateReader>();
            services.AddTransient<IScrollWriter, ScrollWriter>();

            services.AddTransient<IScribe, Scribe>();
            services.AddTransient<IToolbox, Toolbox>();

            services.AddTransient<App>();
        }
    }
}
