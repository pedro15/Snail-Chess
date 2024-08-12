using static SnailChess.Client.Perft.PerftSuiteFactory;
using SnailChess.Core;
using SnailChess.Client.Perft;

namespace SnailChess.Client.Internal.Common.Commands
{
    [Command("sperft" , "executes complete perft suite test of the move generator")]
	internal sealed class PerftSuiteCommand : ICommand
	{
		private readonly CommonClient Client;
		public PerftSuiteCommand(CommonClient _client) => Client = _client;
		
		private PerftRunner runner = null;

		public void Execute(string[] _args)
		{
			Client.stream.Write("Starting Perft Suite ......\n");
			System.Threading.Thread.Sleep(500);
			System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
			ulong expected_value;
			PerftSuiteTest current_test;
			for (int i = 0; i < suiteTests.Length; i++)
			{
				current_test = suiteTests[i];
				Client.stream.Write($"===================== [-- TEST: {i+1}/{suiteTests.Length} --] ==  {current_test.fen}\n");
				for (int j = 0; j < current_test.expected.Length; j++)
				{
					expected_value = current_test.expected[j].Item2;
					Client.stream.Write($" ======== [depth: {current_test.expected[j].Item1} - expected: {expected_value}] ======== \n");

					runner = new PerftRunner(Notation.ParseFEN(current_test.fen));
					runner.RunPerft(current_test.expected[j].Item1 , (move,count) => 
					{
						Client.stream.Write(string.Format("{0,-20}{1,25}{2,6} s\n", $"{move}: {count}", "- time:" , runner.elapsedTime.ToString("N3")));
						return true;
					});

					if (runner.totalNodes != expected_value)
					{
						Client.stream.Write($">> Error! - total nodes was: {runner.totalNodes} but expected value is: {expected_value}\n\n");
						return;
					}else 
					{
						Client.stream.Write(string.Format("{0,-20}{1,25}{2,6} s\n", "> Completed!", "- time:" , runner.elapsedTime.ToString("N3")));
					}

				}
			}

			Client.stream.Write($">> Perft suite completed! in {stopwatch.ElapsedMilliseconds / 1000f} s \n");
			stopwatch.Stop();
		}	

		public void Init() 
		{ 
			PerftSuiteFactory.Init();
		}
	}
}