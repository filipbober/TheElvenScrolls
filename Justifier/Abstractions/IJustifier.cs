namespace Justifier.Abstractions
{
    public interface IJustifier
    {
        string Justify(string text, int width);
        string JustifySingleLine(ref string left, int width);
        int PredictLength(string text);
    }
}
