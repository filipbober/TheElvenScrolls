// Copyright (C) 2017 Filip Cyrus Bober

using System;
using System.Globalization;
using Justifier;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using Justifier.Exceptions;
using Microsoft.Extensions.Logging;
using Templater;
using Templater.Exceptions;

namespace TheElvenScrolls
{
    class Program
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
                Logger.LogCritical(-1, ex, "Creating scroll failed due to Justifier exception");
            }
            catch (TemplaterException ex)
            {
                Logger.LogCritical(-1, ex, "Creating scroll failed due to Templater exception");
            }
            catch (FileNotFoundException ex)
            {
                Logger.LogCritical(-1, ex, "File not found");
            }
            catch (Exception ex)
            {
                Logger.LogCritical(-1, ex, "Creating scroll failed due to unexpected exception");
            }

            Console.ReadKey();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(ApplicationLogging.LoggerFactory);
            services.AddLogging();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            services.AddOptions();

            services.Configure<AppSettings>(configuration);

            services.Configure<JustifierSettings>(configuration.GetSection("justifierSettings"));
            services.AddScoped(cfg => cfg.GetService<IOptionsSnapshot<JustifierSettings>>().Value);

            services.AddTransient<IJustifier, Justifier.Justifier>();
            services.AddTransient<ITemplater, Templater.Templater>();

            services.AddTransient<App>();
        }
    }
}