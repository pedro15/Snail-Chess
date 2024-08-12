namespace SnailChess.Client.Internal.Common.Commands
{
    [Command("quit" , "stops client execution")]
	internal sealed class QuitCommand : ICommand
	{
		private readonly CommonClient Client;
		public QuitCommand(CommonClient _client) => Client = _client;

		public void Execute(string[] _args)
		{
            Client.Stop();
        }
		
		public void Init(){ }
	}
}