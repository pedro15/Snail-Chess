namespace SnailChess.Client.Internal.UCI.Commands
{
	[Command("quit")]
	internal sealed class QuitUCICommand : ICommand
	{
		private readonly UCIClient Client;
		public QuitUCICommand(UCIClient _client) => Client = _client;

		public void Execute(string[] _args)
		{
            Client.Stop();
		}
        
		public void Init() { }
	}
}