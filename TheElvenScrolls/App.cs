using Justifier;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Templater;

namespace TheElvenScrolls
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly AppSettings _settings;
        private readonly IJustifier _justifier;
        private readonly ITemplater _templater;

        private readonly JustifierSettings _configTest;

        public App(ILogger<App> logger, IOptionsSnapshot<AppSettings> settings, IOptions<JustifierSettings> configTest, IJustifier justifier, ITemplater templater)
        {
            _logger = logger;
            _settings = settings.Value;
            _justifier = justifier;
            _templater = templater;
            _configTest = configTest.Value;
        }

        public void Run()
        {
            Console.WriteLine("Copyright (C) 2017 Filip Cyrus Bober");
            Console.WriteLine("The Elven Scrolls ASCII letter generator");

            const string text = @"Test justify paragraph

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
            Nameless here for evermore.";

            Console.WriteLine("Raw text:");
            Console.WriteLine(text);

            var justified = _justifier.Justify(text, 60);
            _logger.LogInformation("Justification finished");
            Console.Write(justified);

            var scroll = _templater.CreateScroll(justified);

            _logger.LogInformation("Scroll ready");
            Console.Write(scroll);
            // ---

            Console.ReadKey();
        }
    }
}
