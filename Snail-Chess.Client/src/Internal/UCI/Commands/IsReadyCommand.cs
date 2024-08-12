namespace SnailChess.Client.Internal.UCI.Commands
{
	[Command("isready")]
	internal sealed class IsReadyCommand : ICommand
	{
		private readonly UCIClient Client;
		public IsReadyCommand(UCIClient _client) => Client = _client;

		public void Execute(string[] _args)
		{
			Client.stream.Write("readyok\n");
		}
		
		public void Init(){ }
	}
}