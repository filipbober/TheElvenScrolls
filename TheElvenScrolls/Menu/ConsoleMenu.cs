using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TheElvenScrolls.Menu.Abstractions;

namespace TheElvenScrolls.Menu
{
    internal sealed class ConsoleMenu : IMenu
    {
        private readonly ILogger _logger;

        public ConsoleMenu(ILogger<ConsoleMenu> logger)
        {
            _logger = logger;
        }

        public void Run(IList<string> menu, Func<string, bool> tryExecute, Action executeDefault)
        {
            LogWhenNull(menu, tryExecute, executeDefault);

            var invalidOption = false;
            var line = string.Empty;
            while (true)
            {
                PrintMenu(menu);

                Console.WriteLine(invalidOption ? "Invalid option. Provide valid option:" : "Provide option: ");

                invalidOption = true;

                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.Write("\b");
                        return;
                    }

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        if (line.Length == 0)
                        {
                            executeDefault?.Invoke();
                            invalidOption = false;
                        }
                        else if (tryExecute != null && tryExecute(line))
                        {
                            invalidOption = false;
                        }

                        line = string.Empty;
                    }
                    else
                    {
                        line = HandleInput(key, line);
                    }

                } while (key.Key != ConsoleKey.Enter);
            }
        }

        public void Wait()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public void Write(string text)
        {
            Console.Write(text);
        }

        public void NewLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }

        public void WriteLine(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }

        private static void PrintMenu(IList<string> menu)
        {
            Console.Clear();

            foreach (var line in menu)
                Console.WriteLine(line);

            Console.WriteLine();
            Console.WriteLine("Enter to run default configuration");
            Console.WriteLine("Esc to exit");
            Console.WriteLine();
        }

        private void LogWhenNull(IList<string> menu, Func<string, bool> tryExecute, Action executeDefault)
        {
            if (menu == null)
                _logger.LogDebug("Menu is null");
            if (tryExecute == null)
                _logger.LogDebug("TryExecute is null");
            if (executeDefault == null)
                _logger.LogDebug("ExecuteDefault is null");
        }

        private static string HandleInput(ConsoleKeyInfo key, string line)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (line.Length <= 0)
                    return line;

                line = line.Substring(0, line.Length - 1);
                Console.Write("\b \b");
            }
            else
            {
                line += key.KeyChar;
                Console.Write(key.KeyChar);
            }
            return line;
        }
    }
}
