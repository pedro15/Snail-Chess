namespace SnailChess.Client.Internal.UCI.Commands
{
	[Command("stop")]
	internal sealed class StopCommand : ICommand
	{
		private readonly UCIClient Client;
		public StopCommand(UCIClient _client) => Client = _client;

		public void Execute(string[] _args)
		{
			Client.engine.AbortSearch();	
		}
		
		public void Init()
		{

		}
	}
}