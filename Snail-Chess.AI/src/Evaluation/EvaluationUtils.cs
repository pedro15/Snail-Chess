using System;
using System.Runtime.CompilerServices;
using SnailChess.Core;
using SnailChess.Core.MoveGen;

namespace SnailChess.AI.Evaluation
{
    public static class EvaluationUtils 
    {        
        public static readonly int[] GAME_PHASE_VALUES =
        { 
            0, // Black (not used)
            0, // White (not used)
            0, // Pawn
            1, // Knight
            1, // Bishop
            2, // Rook
            4, // Queen
            0, // King
        };
        
        public static readonly int TOTAL_GAME_PHASE = (GAME_PHASE_VALUES[Piece.Pawn] * 16) + (GAME_PHASE_VALUES[Piece.Knight] * 4) + (GAME_PHASE_VALUES[Piece.Bishop] * 4) + 
                                (GAME_PHASE_VALUES[Piece.Rook] * 4)  + (GAME_PHASE_VALUES[Piece.Queen]  * 2);
        
        /// <summary>
        /// Isolated Pawns masks [Square]
        /// </summary>
		public static readonly ulong[] IsolatedPawns_masks;
        /// <summary>
        /// Passed Pawns masks [Color][Square]
        /// </summary>
		public static readonly ulong[][] PassedPawns_masks;

        /// <summary>
        /// Free pawns masks [Color][Square]
        /// </summary>
        public static readonly ulong[][] FreePawns_masks;
        
        /// <summary>
        /// King zones [Square]
        /// </summary>
        public static readonly ulong[] King_zone;
        
        

        public static void Init(){ }
        
        static EvaluationUtils()
        {
            IsolatedPawns_masks = new ulong[64];

            PassedPawns_masks = new ulong[2][];
            PassedPawns_masks[Piece.Black] = new ulong[64];
            PassedPawns_masks[Piece.White] = new ulong[64];

            FreePawns_masks = new ulong[2][];
            FreePawns_masks[Piece.Black] = new ulong[64];
            FreePawns_masks[Piece.White] = new ulong[64];

            King_zone = new ulong[64];

            byte sq_rank;
            byte sq_file;
            byte king_sq;

            for (byte rank = 0; rank < 8; rank++)
            {
                for (byte file = 0; file < 8; file++)
                {
                    byte sq = BoardUtils.GetSquare(in file, in rank);
                    sq_rank = BoardUtils.SquareToRank(sq);
                    sq_file = BoardUtils.SquareToFile(sq);


                    // --------- Isolated masks ---------------------------------
                    IsolatedPawns_masks[sq] = MoveGenerator.FileRankMask(file - 1, -1);
                    IsolatedPawns_masks[sq] |= MoveGenerator.FileRankMask(file + 1, -1);

                    // System.Console.WriteLine($"Isolated Pawn ({(BoardSquare)sq}):\n{Notation.PrintBitboard(IsolatedPawns_masks[sq])}");

                    // --------- Black passed masks ------------------------------
                    PassedPawns_masks[Piece.Black][sq] |= MoveGenerator.FileRankMask(file -1, -1);
                    PassedPawns_masks[Piece.Black][sq] |= MoveGenerator.FileRankMask(file, -1);
                    PassedPawns_masks[Piece.Black][sq] |= MoveGenerator.FileRankMask(file + 1, -1);
                    
                    // remove unneded bits
					for (int i = 0; i < (8 - rank); i++)
						PassedPawns_masks[Piece.Black][sq] &= ~BoardUtils.RANKS[BoardUtils.SquareToRank((byte)((7 - i) * 8 + file))];
                    
                    FreePawns_masks[Piece.Black][sq] = PassedPawns_masks[Piece.Black][sq] & BoardUtils.FILES[BoardUtils.SquareToFile(sq)];

                    // System.Console.WriteLine($"Free Pawn (b)({(BoardSquare)sq}):\n{Notation.PrintBitboard(FreePawns_masks[Piece.Black][sq])}");
                    // System.Console.WriteLine($"Passed Pawn (b)({(BoardSquare)sq}):\n{Notation.PrintBitboard(PassedPawns_masks[Piece.Black][sq])}");

                    // --------- White passed masks ------------------------------
                    PassedPawns_masks[Piece.White][sq] |= MoveGenerator.FileRankMask(file -1, -1);
                    PassedPawns_masks[Piece.White][sq] |= MoveGenerator.FileRankMask(file +1, -1);
                    PassedPawns_masks[Piece.White][sq] |= MoveGenerator.FileRankMask(file, -1);
                    // remove unneded bits
					for (int i = 0; i < rank + 1; i++)
						PassedPawns_masks[Piece.White][sq] &= ~BoardUtils.RANKS[BoardUtils.SquareToRank((byte)(i * 8 + file))];

                    FreePawns_masks[Piece.White][sq] = PassedPawns_masks[Piece.White][sq] & BoardUtils.FILES[BoardUtils.SquareToFile(sq)];

                    // System.Console.WriteLine($"Free Pawn (w)({(BoardSquare)sq}):\n{Notation.PrintBitboard(FreePawns_masks[Piece.White][sq])}");
                    // System.Console.WriteLine($"Passed Pawn (w)({(BoardSquare)sq}):\n{Notation.PrintBitboard(PassedPawns_masks[Piece.White][sq])}");

                    // ----- KingZone ----------
                    king_sq = sq;
                    King_zone[king_sq] = MoveGenerator.KingMoves(king_sq);
                    if (sq_rank == 0)
                        sq_rank++;
                    else if (sq_rank == 7)
                        sq_rank--;

                    if (sq_file == 0)
                        sq_file++;
                    else if (sq_file == 7)
                        sq_file--;

                    king_sq = BoardUtils.GetSquare(in sq_file, in sq_rank);
                    King_zone[king_sq] = MoveGenerator.KingMoves(king_sq) | 1UL << king_sq;
                    King_zone[king_sq] &= ~(1UL << sq);

                    //Console.WriteLine($"King_Zone:{(BoardSquare)sq}\n{Notation.PrintBitboard(King_zone[king_sq])}");
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int InterpolateScores(int _mg_score, int _eg_score, int _phase)
        {
            if (_phase > TOTAL_GAME_PHASE) _phase = TOTAL_GAME_PHASE;
            return (_mg_score * _phase + _eg_score * (TOTAL_GAME_PHASE - _phase)) / TOTAL_GAME_PHASE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong PawnsRayAttacks(byte _color, in ulong _pawns)
        {
            switch(_color)
            {
                case Piece.White:
                    return (BoardUtils.MapNorthEast(in _pawns) & BoardUtils.MASK_PATTERN_FILE_A) |
                              (BoardUtils.MapNorthWest(in _pawns) & BoardUtils.MASK_PATTERN_FILE_H);
                
                case Piece.Black:
                    return (BoardUtils.MapSouthEast(in _pawns) & BoardUtils.MASK_PATTERN_FILE_H) |
                              (BoardUtils.MapSouthWest(in _pawns) & BoardUtils.MASK_PATTERN_FILE_A);
            }

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetAttackMap(in ChessBoard _board, byte _color)
        {
            ulong attacks = 0UL, bb = 0UL, Occupancy = _board.Occupancy;
            byte sq;
            for (byte piece = Piece.Pawn; piece < Piece.King; piece++)
            {
                bb = _board.bitboards[piece] & _board.bitboards[_color];

                switch(piece)
                {
                    case Piece.Pawn:
                    attacks |= PawnsRayAttacks(_color, in  bb);
                    break;

                    case Piece.Knight:
                    while(bb != 0)
                    {
                        sq = BitUtils.BitScan(bb);
                        attacks |= MoveGenerator.KnightMoves(sq);
                        bb = BitUtils.PopLsb(bb);
                    }
                    break;

                    case Piece.Bishop:
                    while(bb != 0)
                    {
                        sq = BitUtils.BitScan(bb);
                        attacks |= MoveGenerator.BishopMoves(in Occupancy, sq);
                        bb = BitUtils.PopLsb(bb);
                    }
                    break;

                    case Piece.Rook:
                    while(bb != 0)
                    {
                        sq = BitUtils.BitScan(bb);
                        attacks |= MoveGenerator.RookMoves(in Occupancy, sq);
                        bb = BitUtils.PopLsb(bb);
                    }
                    break;

                    case Piece.Queen:
                    while(bb != 0)
                    {
                        sq = BitUtils.BitScan(bb);
                        attacks |= MoveGenerator.QueenMoves(in Occupancy, sq);
                        bb = BitUtils.PopLsb(bb);
                    }
                    break;
                }

            }
            return attacks;
        }       
    }
}