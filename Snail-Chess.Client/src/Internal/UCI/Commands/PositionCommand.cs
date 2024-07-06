using SnailChess.Core;

namespace SnailChess.Client.Internal.UCI.Commands
{
	[Command("position")]
	internal sealed class PositionCommand : ICommand
	{	
		private readonly UCIClient Client = null;
		public PositionCommand(UCIClient _client) => Client = _client;

		public void Init(){ }

		public void Execute(string[] _args)
		{
			if (_args.Length < 2 || Client.engine.IsBusy) return; 

			int moves_index = -1;
            string fen = string.Empty;

			void GrabMovesIndex()
			{
				// grab the moves startindex (if any)
				for (int i = 1; i < _args.Length; i++)
				{
					if (_args[i] == "moves")
					{
						moves_index = i;
						break;
					}
				}
			}

			if (_args[1] == "startpos")
			{
				fen = Notation.POSITION_DEFAULT;
				GrabMovesIndex();
			}else if (_args[1] == "kiwipete")
			{
				fen = Notation.POSITION_KIWIPETE;
				GrabMovesIndex();
			}else if (_args[1] == "fen" && _args.Length >= 5)
			{
				// try build the fen from the raw args
				// and also grab the moves startindex (if any)
				for (int i = 2; i < _args.Length; i++)
				{
					if (_args[i] == "moves")
					{
						moves_index = i;
						break;
					}

					fen += $"{_args[i]} ";
				}
			}

			if (Notation.IsValidFEN(fen))
			{
				BoardPosition pos = Notation.ParseFEN(fen);
            	Client.board.LoadPosition(in pos);
				Client.engine.AILoadPosition(in pos);
			}else 
			{
				Client.stream.Write($"invalid fen: {fen}\n");
				return;
			}

			if (moves_index != -1)
			{
				Move[] legal_moves;
				Move loaded_move;
				
				for (int i = moves_index + 1; i < _args.Length; i++)
				{	
					legal_moves = Move.GetLegalMoves(Client.board.ExportPosition());
					loaded_move = Move.FromLAN(_args[i], in legal_moves);

					if(!(Client.board.MakeMove(in loaded_move) && Client.engine.AIMakeMove(in loaded_move)))
					{
						Client.stream.Write($"illegal move: {_args[i]}\n");
						break;
					}
				}
			}
		}

	}
}