using System.Threading.Tasks;
using SnailChess.Client.Perft;
using SnailChess.Core;
using SnailChess.AI.Search;

namespace SnailChess.Client.Internal.UCI.Commands
{
    [Command("go")]
	internal sealed class GoCommand : ICommand
	{
		private readonly UCIClient Client;
		public GoCommand(UCIClient _client) => Client = _client;
		private PerftRunner perftRunner;
		
		public void Execute(string[] _args)
		{
			if (Client.engine.IsBusy) return;
			Task.Run(() => RunSearch(in _args));
		}

		private void RunSearch(in string[] _args)
		{
				// Perft test
				int peft_depth = Client.GetParamValue<int>("perft", _args, -1);
				if (peft_depth >= 0)
				{
					perftRunner = new PerftRunner(Client.board.ExportPosition());
					perftRunner.RunPerft(peft_depth, (move, count) =>
					{
						Client.stream.Write($"{move}: {count}\n");
						return true;
					});

					float mlps = perftRunner.totalNodes / perftRunner.elapsedTime / 1000000f;
					Client.stream.Write($"\nmoves count: {perftRunner.totalNodes}  time: {perftRunner.elapsedTime:N3} s  speed: {mlps:N} ML/s\n");
					return;
				}

				SearchResults results = new SearchResults();
				void PrintBestMove() => Client.stream.Write($"bestmove {results.bestMove}\n");

				// Fixed depth
				uint search_depth = Client.GetParamValue<uint>("depth", _args, 0);
				if (Client.GetParamValue<string>("go", _args, string.Empty).Equals("infinite")) search_depth = 99999999;

				if (search_depth > 0)
				{
					results = Client.engine.FindBestMove(SearchType.FixedDepth, search_depth);
					PrintBestMove();
					return;
				}

				// Fixed movetime
				uint search_movetime = Client.GetParamValue<uint>("movetime", _args, 0);
				if (search_movetime > 0)
				{
					results = Client.engine.FindBestMove(SearchType.FixedTime, search_movetime);
					PrintBestMove();
					return;
				}

				// Fixed nodes
				uint search_nodes = Client.GetParamValue<uint>("nodes", _args, 0);
				if (search_nodes > 0)
				{
					results = Client.engine.FindBestMove(SearchType.FixedNodes,search_nodes);
					PrintBestMove();
					return;
				}

				// tournament-like run
				uint wtime = Client.GetParamValue<uint>("wtime", _args, 0);
				uint btime = Client.GetParamValue<uint>("btime", _args, 0);
				if (wtime > 0 && btime > 0)
				{
					uint winc = Client.GetParamValue<uint>("winc", _args, 0);
					uint binc = Client.GetParamValue<uint>("binc", _args, 0);
					results = Client.engine.FindBestMove(wtime, winc, btime, binc);
					PrintBestMove();
					return;
				}
				
				Client.stream.Write("unknown parameters\n");
		}


		public void Init()
		{
			Client.engine.OnSearchUpdated += (int _depth, ulong _nodes, int _score, bool _mate, uint _speed , uint _time, Move[] _pv) => 
			{
				if(_mate)
				{
					Client.stream.Write($"info depth {_depth} score mate {_score} nodes {_nodes} time {_time} nps {_speed}");
				}else 
				{
					Client.stream.Write($"info depth {_depth} score cp {_score} nodes {_nodes} time {_time} nps {_speed}");
				}

				if (_pv.Length > 0)
				{
					Client.stream.Write(" pv ");
					for (int i = 0; i < _pv.Length; i++)
						Client.stream.Write($"{_pv[i]} ");
				}

				Client.stream.Write("\n");
			};
        }
	}
}