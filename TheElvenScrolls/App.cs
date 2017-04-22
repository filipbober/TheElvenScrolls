using Justifier;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace TheElvenScrolls
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly AppSettings _settings;
        private readonly IJustifier _justifier;

        private readonly JustifierSettings _configTest;

        public App(ILogger<App> logger, IOptionsSnapshot<AppSettings> settings, IOptions<JustifierSettings> configTest, IJustifier justifier)
        {
            _logger = logger;
            _settings = settings.Value;
            _justifier = justifier;
            _configTest = configTest.Value;
        }

        public void Run()
        {
            Console.WriteLine("Copyright (C) 2017 Filip Cyrus Bober");
            Console.WriteLine("The Elven Scrolls ASCII letter generator");

            var text = @"Test justify paragraph

            Test justify paragraph

            Test justify paragraph

            LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong Short Short

            Short Short LongLongLongLongLongLongLongLongLongLongLongLongLong Short Short

            Once         upon a midnight dreary, while I pondered, weak and weary,
            Over many a quaint and curious volume of forgotten lore,
            While I nodded, nearly napping, suddenly there came a tapping,
            As of someone gently rapping, tapping at my chamber door.
            'Tis some visitor, I muttered, tapping at my chamber door-
            Only this, and nothing more.

            Dwa slowa.

            Ah, distinctly I remember it was in a bleak December,
            And each separate dying ember wrought its ghost upon the floor.
            Eagerly I wished the morrow; -vainly I had sought to borrow
            From my books surcease of sorrow - sorrow for the lost Lenore -
             For the rare and radiant maiden whom the angels name Lenore -
             Nameless here for evermore.
             ";

            Console.WriteLine("Raw text:");
            Console.WriteLine(text);

            _logger.LogDebug("Test");

            var justified = _justifier.Justify(text, 30);

            Console.WriteLine("----------------");

            Console.WriteLine("Justified:");
            Console.Write(justified);

            _logger.LogInformation("Justification finished");
            Console.ReadKey();
        }
    }
}
