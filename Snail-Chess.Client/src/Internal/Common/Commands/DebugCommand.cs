#if DEBUG
using System.Diagnostics;
using System;
using SnailChess.Core;
using SnailChess.Core.MoveGen;
using SnailChess.AI;

namespace SnailChess.Client.Internal.Common.Commands
{
	[Command("debug")]
	internal sealed class DebugCommand : ICommand
	{
		private readonly CommonClient Client;
		public DebugCommand(CommonClient _client) => Client = _client;

		public void Execute(string[] _args)
		{
			
		}
		
		public void Init() { }
	}
}
#endif