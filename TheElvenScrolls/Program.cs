// Copyright (C) 2017 Filip Cyrus Bober

using System;
using TheElvenScrolls.Justification;

namespace TheElvenScrolls
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Copyright (C) 2017 Filip Cyrus Bober");
            Console.WriteLine("The Elven Scrolls ASCII letter generator");            

            var text = @"Test justify paragraph

Test justify paragraph

Test justify paragraph

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

            var justifier = new Justifier();
            var justified = justifier.Justify(text, 30);            

            Console.WriteLine("----------------");

            Console.WriteLine("Justified:");
            Console.Write(justified);

            Console.ReadKey();
        }
    }
}