using SnailChess.Core;
using SnailChess.AI.Search;
using SnailChess.AI.Evaluation;
using SnailChess.AI.Personalities;

namespace SnailChess.Client.Internal.UCI
{
	internal sealed class UCIClient : ClientBase
	{
		protected override ICommand[] GetCommands()
		{
			return new ICommand[]
            {
				#if DEBUG
				new Commands.DebugUCICommand(this),
				#endif
                new Commands.QuitUCICommand(this),
				new Commands.UCINewGameCommand(this),
				new Commands.IsReadyCommand(this),
				new Commands.PositionCommand(this),
				new Commands.GoCommand(this),
				new Commands.StopCommand(this),
				new Commands.SetOptionCommand(this),
				new Commands.DisplayCommand(this),
				new Commands.EvaluateCommand(this),
				new Commands.PolyKeyCommand(this),
            };
		}
		
		private BotPersonality[] LEVELS = 
		{
			BotPersonality.BEGINNER,
			BotPersonality.EASY,
			BotPersonality.MEDIUM,
			BotPersonality.HARD,
			BotPersonality.VERY_HARD,
			BotPersonality.MAX
		};

		private int levelIndex = -1;
		public ChessBoard board = null;
		public SearchEngine engine = null;

		public UCIClient()
		{
			EvaluationUtils.Init();
			engine = new SearchEngine();
			board = new ChessBoard();
			levelIndex = GetLevelMaximumIndex();
		}
		
		protected override void OnInit() { } 

		public void UCI_Init()
		{
			stream.Write($"id name {ClientConstants.ENGINE_NAME} {ClientConstants.VERSION}\n");
			stream.Write("id author Pedro Duran\n");
			WriteOptions();
			stream.Write("uciok\n");
		}

		private void WriteOptions()
		{
			stream.Write($"option name level type spin default {GetLevelMaximumIndex()} min {GetLevelMinimumIndex()} max {GetLevelMaximumIndex()}\n");
		}

		public BotPersonality GetCurrentPersonality()
		{
			if (levelIndex >= 0 && levelIndex < LEVELS.Length)
			{
				return LEVELS[levelIndex];
			}
			return BotPersonality.MAX;
		}

		public int GetLevelMinimumIndex() => 0;
		public int GetLevelMaximumIndex() => LEVELS.Length -1;

		public void SetLevelIndex(int _level)
		{
			if (_level >= GetLevelMinimumIndex() && _level <= GetLevelMaximumIndex())
			{
				levelIndex = _level;
			}
			stream.Write($"info string selected level is now {levelIndex}\n");
		}
	}
}