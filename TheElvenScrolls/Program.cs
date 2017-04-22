﻿// Copyright (C) 2017 Filip Cyrus Bober

using Justifier;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

            services.AddTransient<IJustifier, Justifier.Justifier>();            

            services.AddTransient<App>();
        }
    }
}