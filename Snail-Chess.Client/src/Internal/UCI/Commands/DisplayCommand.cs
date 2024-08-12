namespace SnailChess.Client.Internal.UCI.Commands
{
	[Command("d")]
	internal sealed class DisplayCommand : ICommand
	{
		private readonly UCIClient Client;
		public DisplayCommand(UCIClient _client) => Client = _client;
		
		public void Execute(string[] _args)
		{
			Client.stream.Write($"{Client.board}\n");
		}
		public void Init() { }
	}
}