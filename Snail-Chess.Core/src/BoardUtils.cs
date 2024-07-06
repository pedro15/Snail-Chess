using System.Runtime.CompilerServices;

namespace SnailChess.Core
{
    public static class BoardUtils
    {
        public const ulong WHITE_SQUARES = 0x55aa55aa55aa55aa;
        public const ulong BLACK_SQUARES = 0xaa55aa55aa55aa55;

        public const ulong MASK_PATTERN_FILE_A  = 0xfefefefefefefefe;
        public const ulong MASK_PATTERN_FILE_H  = 0x7f7f7f7f7f7f7f7f;
        public const ulong MASK_PATTERN_FILE_AB = 0xfcfcfcfcfcfcfcfc;
        public const ulong MASK_PATTERN_FILE_GH = 0x3f3f3f3f3f3f3f3f;

        public const ulong MASK_CASTLE_QUEENSIDE_WHITE = 0x00000000000000e;
        public const ulong MASK_CASTLE_QUEENSIDE_BLACK = 0xe00000000000000;
        public const ulong MASK_CASTLE_KINGSIDE_WHITE  = 0x0000000000000060;
        public const ulong MASK_CASTLE_KINGSIDE_BLACK  = 0x6000000000000000;

        // [FileIndex] 0 ... 7
        public static readonly ulong[] FILES = 
        {
            0x0101010101010101, // A 
            0x0202020202020202, // B 
            0x0404040404040404, // C
            0x0808080808080808, // D 
            0x1010101010101010, // E
            0x2020202020202020, // F 
            0x4040404040404040, // G
            0x8080808080808080  // H
        };

        // [RankIndex] 0 ... 7
        public static readonly ulong[] RANKS = 
        {
            0x00000000000000ff,
            0x000000000000ff00,
            0x0000000000ff0000,
            0x00000000ff000000,
            0x000000ff00000000,
            0x0000ff0000000000,
            0x00ff000000000000,
            0xff00000000000000
        };

        /// <summary>
        /// Promotion area indexed by [color]
        /// </summary>
        public static readonly ulong[] PROMOTION_AREA = 
        {
            0x00000000000000ff,
            0xff00000000000000
        };

        // [SquareIndex] 0 ... 63
        public static readonly byte[] CENTER_DISTANCE = 
        {
            6, 5, 4, 3, 3, 4, 5, 6,
            5, 4, 3, 2, 2, 3, 4, 5,
            4, 3, 2, 1, 1, 2, 3, 4,
            3, 2, 1, 0, 0, 1, 2, 3,
            3, 2, 1, 0, 0, 1, 2, 3,
            4, 3, 2, 1, 1, 2, 3, 4,
            5, 4, 3, 2, 2, 3, 4, 5,
            6, 5, 4, 3, 3, 4, 5, 6
        };
        
        // [SquareIndex_1][SquareIndex_2]
        // 64 x 64 table = 4 KByte
        public static readonly byte[][] DISTANCE;

        // [Sq][playerIndex]
        public static readonly CastleRights[][] CASTLE_RIGHTS_ROOK;

        public static void Init(){ }
        
        static BoardUtils()
        {
            byte ComputeDistance(byte _sq1, byte _sq2)
            {
                byte file1, file2, rank1, rank2;
                byte rankDistance, fileDistance;
                file1 = SquareToFile(_sq1); rank1 = SquareToRank(_sq1);
                file2 = SquareToFile(_sq2); rank2 = SquareToRank(_sq2);
                rankDistance = (byte)System.Math.Abs(rank2 - rank1);
                fileDistance = (byte)System.Math.Abs(file2 - file1);
                return System.Math.Max(rankDistance,fileDistance);
            }

            DISTANCE = new byte[64][];
            for (byte sq_a = 0; sq_a < 64; sq_a++)
            {
                DISTANCE[sq_a] = new byte[64];
                for (byte sq_b = 0; sq_b < 64; sq_b++)
                {
                    DISTANCE[sq_a][sq_b] = ComputeDistance(sq_a, sq_b);
                }
            }

            CASTLE_RIGHTS_ROOK = new CastleRights[64][];
            for (byte sq = 0; sq < 64; sq++)
            {
                CASTLE_RIGHTS_ROOK[sq] = new CastleRights[2];

                for (byte color = Piece.Black; color <= Piece.White; color++)
                {
                    CASTLE_RIGHTS_ROOK[sq][color] = CastleRights.None;
                }
            }
            
            CASTLE_RIGHTS_ROOK[((byte)BoardSquare.a8)][Piece.Black] = CastleRights.QueenSide_Black;
            CASTLE_RIGHTS_ROOK[((byte)BoardSquare.h8)][Piece.Black] = CastleRights.KingSide_Black;
            CASTLE_RIGHTS_ROOK[((byte)BoardSquare.a1)][Piece.White] = CastleRights.QueenSide_White;
            CASTLE_RIGHTS_ROOK[((byte)BoardSquare.h1)][Piece.White] = CastleRights.KingSide_White;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MapSouth(in ulong _bb) => _bb >> 8;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MapSouthWest(in ulong _bb) => _bb >> 7;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MapSouthEast(in ulong _bb) => _bb >> 9;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MapNorth(in ulong _bb) => _bb << 8;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MapNorthWest(in ulong _bb) => _bb << 7;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MapNorthEast(in ulong _bb) => _bb << 9;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SquareToFile(in byte _sq) => (byte)(_sq & 7);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SquareToRank(in byte _sq) => (byte)(_sq >> 3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SquareColor(in byte _sq) => (byte)((BLACK_SQUARES >> _sq) & 1);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SquareInvert(in byte _sq) => (byte)(_sq ^ 56);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetSquare(in byte _file, in byte _rank) => (byte)((_rank << 3) + _file);
    }
}