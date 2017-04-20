// Copyright (C) 2017 Filip Cyrus Bober

using Justifier;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

namespace TheElvenScrolls
{
    class Program
    {
        static void Main(string[] args)
        {            
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.GetService<App>().Run();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(new LoggerFactory()
                .AddConsole());
            serviceCollection.AddLogging();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json", optional: false)
                .Build();
            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration.GetSection("Configuration"));
            serviceCollection.Configure<JustifierSettings>(configuration.GetSection("JustifierSettings"));

            serviceCollection.AddTransient<IJustifier, Justifier.Justifier>();

            serviceCollection.AddTransient<App>();
        }
    }
}