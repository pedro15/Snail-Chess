using SnailChess.Core;
using SnailChess.Core.Hashing;
using SnailChess.Core.MoveGen;

namespace SnailChess.Client.Internal.Common
{
	internal sealed class CommonClient : ClientBase
	{
		protected override ICommand[] GetCommands()
		{
            return new ICommand[]
            {
				#if DEBUG
				new Commands.DebugCommand(this),
				#endif
				new Commands.HelpCommand(this),
				new Commands.QuitCommand(this),
				new Commands.UCICommand(this),
				new Commands.PerftSuiteCommand(this),
				new Commands.MagicsCommand(this),
            };
        }
				
		protected override void OnInit()
		{
			BoardHash.Init();
			PawnHash.Init();
			MoveGenerator.Init();
			BitUtils.Init();
			BoardUtils.Init();
		}
	}
}