namespace SnailChess.Core
{
    [System.Serializable]
    #pragma warning disable CS0659
    public struct BoardPosition
    #pragma warning restore CS0659
    {
        public const byte MAX_PLY = 128;
        public ulong[] bitboards;
        public ulong key;
        public uint key_pawns;
        public byte sideToMove;
        public CastleRights castleRights;
        public byte ep_square;
        public byte halfmoves;
        public byte fullmoves;

        public void Dispose()
        {
            bitboards[Piece.White] = 0UL;
            bitboards[Piece.Black] = 0UL;
            bitboards[Piece.Pawn] = 0UL;
            bitboards[Piece.Knight] = 0UL;
            bitboards[Piece.Bishop] = 0UL;
            bitboards[Piece.Rook] = 0UL;
            bitboards[Piece.Queen] = 0UL;
            bitboards[Piece.King] = 0UL;

            sideToMove = Piece.White;
            castleRights = CastleRights.None;
            ep_square = 0;
            halfmoves = 0;
            fullmoves = 0;

            key_pawns = 0;
            key = 0UL;
        }

        public static BoardPosition EmptyPosition()
        {
            return new BoardPosition()
            {
                bitboards = new ulong[8],
                sideToMove = Piece.White,
                castleRights = CastleRights.None,
                ep_square = 0,
                halfmoves = 0,
                fullmoves = 0,
                key_pawns = 0,
                key = 0UL,
            };
        }

		public override bool Equals(object obj)
		{
            if (obj is BoardPosition pos)
            {
                return pos.sideToMove == sideToMove && pos.ep_square == ep_square &&
                       pos.castleRights == castleRights && pos.halfmoves == halfmoves && 
                       pos.key == key && pos.key_pawns == key_pawns && pos.bitboards != null && pos.bitboards[Piece.White] == bitboards[Piece.White] && 
                       pos.bitboards[Piece.Black] == bitboards[Piece.Black] && 
                       pos.bitboards[Piece.Pawn] == bitboards[Piece.Pawn] && 
                       pos.bitboards[Piece.Knight] == bitboards[Piece.Knight] && 
                       pos.bitboards[Piece.Bishop] == bitboards[Piece.Bishop] && 
                       pos.bitboards[Piece.Rook] == bitboards[Piece.Rook] && 
                       pos.bitboards[Piece.Queen] == bitboards[Piece.Queen] && 
                       pos.bitboards[Piece.King] == bitboards[Piece.King];
            }
            return false;
        }
    }
}