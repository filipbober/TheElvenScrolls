﻿namespace Justifier.Exceptions
{
    public class JustifierException : System.Exception
    {
        public JustifierException()
        {
        }

        public JustifierException(string message) : base(message)
        {
        }

        public JustifierException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }
}
