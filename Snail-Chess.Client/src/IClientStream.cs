namespace SnailChess.Client
{
    public interface IClientStream
    {
        void Write(string _message);
        string GetInput();
    }
}