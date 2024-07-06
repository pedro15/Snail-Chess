using SnailChess.AI.Personalities;

namespace SnailChess.Client.Internal.UCI.Commands
{
    [Command("ucinewgame")]
	internal sealed class UCINewGameCommand : ICommand
	{
        private readonly UCIClient Client;

        public UCINewGameCommand(UCIClient _client)
        {
            Client = _client;
        }
        
		public void Execute(string[] _args)
		{
            ExecuteNewGame();
		}

		public void Init()
        { 
            ExecuteNewGame();
        }

        private void ExecuteNewGame()
        {
            BotPersonality personality = Client.GetCurrentPersonality();
            Client.engine.AINewGame();
            Client.engine.AILoadParams(personality.searchOptions, personality.evaluationParams);
        }
	}
}