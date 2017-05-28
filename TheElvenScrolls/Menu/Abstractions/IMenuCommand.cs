namespace TheElvenScrolls.Menu.Abstractions
{
    public interface IMenuCommand
    {
        string Description { get; }
        void Execute();
    }
}
