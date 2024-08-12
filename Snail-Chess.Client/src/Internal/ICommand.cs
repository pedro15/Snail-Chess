namespace SnailChess.Client.Internal
{
    internal interface ICommand
    {
        void Init();
        void Execute(string[] _args);
    }
}