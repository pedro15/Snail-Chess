namespace SnailChess.Core.Hashing
{
    public static class BoardHash
    {
        // [sq] [pieceType] [playerIndex] 
        public static readonly ulong[][][] piecesHashKeys;
        // [castleIndex]
        public static readonly ulong[] castleHashKeys;
        // [PlayerColor]
        public static readonly ulong[] sideToMoveHashKeys;
        // [En-passant file]
        public static readonly ulong[] epHashKeys;

        public static void Init() { }

        static BoardHash()
        {

            sideToMoveHashKeys = new ulong[2];
            sideToMoveHashKeys[Piece.White] = RandomBits.Random64();
            sideToMoveHashKeys[Piece.Black] = RandomBits.Random64();

            castleHashKeys = new ulong[(byte)CastleRights.All + 1];
            castleHashKeys[(byte)CastleRights.KingSide_Black] = RandomBits.Random64();
            castleHashKeys[(byte)CastleRights.KingSide_White] = RandomBits.Random64();
            castleHashKeys[(byte)CastleRights.QueenSide_Black] = RandomBits.Random64();
            castleHashKeys[(byte)CastleRights.QueenSide_White] = RandomBits.Random64();

            epHashKeys = new ulong[8];
            for (byte file = 0; file < 8; file++)
            {
                epHashKeys[file] = RandomBits.Random64();
            }

            piecesHashKeys = new ulong[64][][];
            for (byte sq = 0; sq < 64; sq++)
            {
                piecesHashKeys[sq] = new ulong[8][];

                for (byte pc = Piece.Pawn; pc <= Piece.King; pc++)
                {
                    piecesHashKeys[sq][pc] = new ulong[2];

                    for (byte color = Piece.Black; color <= Piece.White; color++)
                    {
                        piecesHashKeys[sq][pc][color] = RandomBits.Random64();
                    }
                }
            }

        }

        public static ulong ComputeHash(in BoardPosition _position)
        {
            ulong hash64 = 0UL;
            
            hash64 ^= sideToMoveHashKeys[_position.sideToMove];
            hash64 ^= castleHashKeys[(byte)_position.castleRights];
            if (_position.ep_square != 0)
                hash64 ^= epHashKeys[BoardUtils.SquareToFile(_position.ep_square)];
            
            for (byte pc = Piece.Pawn; pc <= Piece.King; pc++)
            {
                for (byte color = Piece.Black; color <= Piece.White; color++)
                {
                    ulong bb = _position.bitboards[pc] & _position.bitboards[color];
                    while(bb != 0)
                    {
                        byte sq = BitUtils.BitScan(bb);
                        bb = BitUtils.PopLsb(bb);
                        
                        hash64 ^= piecesHashKeys[sq][pc][color];
                    }
                }
            }

            return hash64;
        }
        
    }
}