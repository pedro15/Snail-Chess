using SnailChess.Core.MoveGen.Magics;
using SnailChess.Core;

namespace SnailChess.Client.Internal.Common.Commands
{
    [Command("magics" , "find magics numbers and print them (b|r)")]
	internal class MagicsCommand : ICommand
	{
        private readonly CommonClient Client;
        public MagicsCommand(CommonClient _client) => Client = _client;

		public void Execute(string[] _args)
		{
            string piece_type = Client.GetParamValue<string>("magics" , _args, string.Empty);
            if (!string.IsNullOrEmpty(piece_type) && (piece_type == "b" | piece_type == "r"))
            {
                MagicEntry[] magics;
                if (piece_type == "b")
                {
                    magics = MagicGenerator.FindMagics(Piece.Bishop);
                }else
                {
                    magics = MagicGenerator.FindMagics(Piece.Rook);
                }

                Client.stream.Write($"{MagicsUtils.PrintEntries(in magics)}\n");
            }else 
            {
                Client.stream.Write("please specify a valid piece type: (b | r)\n");
            }
		}

		public void Init(){ }
	}
}