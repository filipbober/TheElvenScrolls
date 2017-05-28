namespace Justifier.Abstractions
{
    public interface IVariableWidthJustifier
    {
        string JustifyNextLine(ref string left, int width);
        int PredictLength(string text);
    }
}