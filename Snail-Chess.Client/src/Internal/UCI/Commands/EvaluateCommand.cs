using SnailChess.AI.Evaluation;
using SnailChess.AI.Personalities;

namespace SnailChess.Client.Internal.UCI.Commands
{
    [Command("eval")]
    internal sealed class EvaluateCommand : ICommand
    {
        private readonly UCIClient Client;
        public EvaluateCommand(UCIClient _client)
        {
            Client = _client;
        }

        BotPersonality personality = BotPersonality.MAX;
        EvaluationController evaluationController = new EvaluationController();

		public void Execute(string[] _args)
		{
            int eval = evaluationController.Evaluate(in Client.board);
            Client.stream.Write($"evaluation is: {eval}\n");		
        }
        
		public void Init()
        { 
            evaluationController.LoadParams(personality.evaluationParams);
        }
    }
}