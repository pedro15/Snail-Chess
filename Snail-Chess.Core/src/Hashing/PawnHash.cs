namespace SnailChess.Core.Hashing
{
    public static class PawnHash
    {
        // [sq] [color]
        public static uint[][] pawnHashKeys;

        public static void Init() { }
        static PawnHash()
        {
            pawnHashKeys = new uint[64][];

            for (byte sq = 0; sq < 64; sq++)
            {
                pawnHashKeys[sq] = new uint[64];
                for (byte color = Piece.Black; color <= Piece.White; color++)
                {
                    pawnHashKeys[sq][color] = (RandomBits.Random() >> 16)  | ((RandomBits.Random() >> 16) << 16);
                }
            }
        }

        public static uint ComputeHash(in BoardPosition _position)
        {
            uint hash32 = 0;
            ulong pawns = _position.bitboards[Piece.Pawn];
            byte sq;

            while(pawns != 0)
            {
                sq = BitUtils.BitScan(pawns);
                pawns = BitUtils.PopLsb(pawns);

                if (BitUtils.Contains(_position.bitboards[Piece.White], sq))
                {
                    hash32 ^= pawnHashKeys[sq][Piece.White];
                }else 
                {
                    hash32 ^= pawnHashKeys[sq][Piece.Black];
                }
            }

            return hash32;
        } 
    }
}