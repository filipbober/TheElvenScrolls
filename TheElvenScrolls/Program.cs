// Copyright (C) 2017 Filip Cyrus Bober

using System.Globalization;
using Justifier;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using Templater;

namespace TheElvenScrolls
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.GetService<App>().Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new LoggerFactory()
                .AddConsole());
            services.AddLogging();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            services.AddOptions();

            services.Configure<AppSettings>(configuration);

            services.Configure<JustifierSettings>(configuration.GetSection("justifierSettings"));
            services.AddScoped(cfg => cfg.GetService<IOptionsSnapshot<JustifierSettings>>().Value);

            services.Configure<Template>(configuration.GetSection("template"));
            services.AddScoped(cfg => cfg.GetService<IOptionsSnapshot<Template>>().Value);

            services.AddTransient<IJustifier, Justifier.Justifier>();
            services.AddTransient<ITemplater, Templater.Templater>();

            services.AddTransient<App>();
        }
    }
}