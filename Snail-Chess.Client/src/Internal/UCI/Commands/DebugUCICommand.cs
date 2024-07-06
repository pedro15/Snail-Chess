using SnailChess.Core;
using SnailChess.AI.Evaluation;

namespace SnailChess.Client.Internal.UCI.Commands
{
    #if DEBUG
    [Command("debug")]
	internal sealed class DebugUCICommand : ICommand
	{
        private readonly UCIClient Client;
        public DebugUCICommand(UCIClient _client)
        {
            Client = _client;
        }

		public void Execute(string[] _args)
		{
            
        }
        
		public void Init()
        { 
        
        }
	}
    #endif
}