namespace Templater.Exceptions
{
    public class TemplaterException : System.Exception
    {
        public TemplaterException()
        {
        }

        public TemplaterException(string message) : base(message)
        {
        }

        public TemplaterException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }
}