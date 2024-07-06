using SnailChess.Client.Internal.UCI;

namespace SnailChess.Client.Internal.Common.Commands
{
    [Command("uci" , "start uci client")]
	internal class UCICommand : ICommand
	{
        private readonly CommonClient Client;
        public UCICommand(CommonClient _client) => Client = _client;

        private UCIClient uciClient = null;

		public void Execute(string[] _args)
		{
            uciClient.UCI_Init();
            uciClient.Run();
            // If uci client is over, our client must be over too!
            Client.Stop();
        }

		public void Init()
		{
            uciClient = new UCIClient();
            uciClient.Init(Client.stream);
		}
	}
}