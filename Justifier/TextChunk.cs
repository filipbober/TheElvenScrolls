namespace Justifier
{
    internal class TextChunk
    {
        public string Text { get; set; }
        public ChunkType Type { get; set; }

        public TextChunk(char letter, ChunkType type)
        {
            Text = letter.ToString();
            Type = type;
        }

        public TextChunk(string text, ChunkType type)
        {
            Text = text;
            Type = type;
        }
    }
}
