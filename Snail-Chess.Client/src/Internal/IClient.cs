namespace SnailChess.Client.Internal
{
    internal interface IClient
    {
        void Init(IClientStream _clientStream);
        void Run();
        void Stop();
    }
}