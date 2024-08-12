using SnailChess.Polyglot;

namespace SnailChess.Client.Internal.UCI.Commands
{
    [Command("poly-key")]
	internal sealed class PolyKeyCommand : ICommand
	{
        private readonly UCIClient Client;
        public PolyKeyCommand(UCIClient _client)
        {
            Client = _client;
        }

		public void Execute(string[] _args)
		{
            ulong poly_hash = PolyglotBook.ComputePolyglotHash(Client.board);
            Client.stream.Write($"{poly_hash:X}\n");
        }

		public void Init()
		{
            
        }
	}
}