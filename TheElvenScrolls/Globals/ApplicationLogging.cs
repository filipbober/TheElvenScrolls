using Microsoft.Extensions.Logging;

namespace TheElvenScrolls.Globals
{
    internal static class ApplicationLogging
    {
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory != null)
                    return _loggerFactory;

                _loggerFactory = new LoggerFactory();
                ConfigureLogger(_loggerFactory);

                return _loggerFactory;
            }
        }

        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();

        private static ILoggerFactory _loggerFactory;

        public static void ConfigureLogger(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
        }
    }
}
