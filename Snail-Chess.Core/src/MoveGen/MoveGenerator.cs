using System.Runtime.CompilerServices;
using static SnailChess.Core.MoveGen.Magics.MagicsUtils;
using SnailChess.Core.MoveGen.Magics;

namespace SnailChess.Core.MoveGen
{
    public static class MoveGenerator
    {
        private static readonly ulong[] sliding_attacks; // bishop, rook, queen
        private static readonly ulong[] pawn_attacks;
        private static readonly ulong[] knight_attacks;
        private static readonly ulong[] king_attacks;

        public static void Init() { }

        static MoveGenerator()
        {
            sliding_attacks = new ulong[294912];
            pawn_attacks = new ulong[128];
            knight_attacks = new ulong[64];
            king_attacks = new ulong[64];
            
            byte sq,player, count = 0;
            int magic_index,blocker_index;
            ulong blocker, relevant_bits, pattern;
            MagicEntry entry = MagicEntry.EMPTY;

			void FillSlidingAttacks(byte _sq, byte _piece)
			{
				pattern = PatternSliding(_piece, _sq);
				switch (_piece)
				{
					case Piece.Bishop:
						count = 9;
						entry = MAGICS_BISHOP[(byte)_sq];
						break;
					case Piece.Rook:
						count = 12;
						entry = MAGICS_ROOK[(byte)_sq];
						break;
				}

				for (blocker_index = 0; blocker_index < (1 << count); blocker_index++)
				{
					blocker = MaskVariation(blocker_index, count, pattern);
					magic_index = GetMagicIndex(in blocker, (byte)(64-count), in entry);
					relevant_bits = PatternSlidingRelevant(_piece, _sq, blocker);
					sliding_attacks[magic_index] = relevant_bits;
				}
			}

			for (sq = 0; sq < 64; sq++)
			{
                knight_attacks[sq] = PatternKnight(sq);
                king_attacks[sq] = PatternKing(sq);

				FillSlidingAttacks(sq, Piece.Rook);
				FillSlidingAttacks(sq, Piece.Bishop);

                for (player = Piece.Black; player <= Piece.White; player++)
                {
                    pawn_attacks[(player * 64) + sq] = PatternPawnAttacks(sq, player);
                }
			}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong PawnQuietMoves(ulong _pawnbb, ulong _emptySquares , byte _color)
        {  
            switch(_color)
            {
                case Piece.White:
                    ulong single_push_w  =  BoardUtils.MapNorth(_pawnbb) & _emptySquares;
                    return single_push_w | (BoardUtils.MapNorth(single_push_w) & _emptySquares & 0x00000000FF000000);

                case Piece.Black:
                    ulong single_push_b = BoardUtils.MapSouth(_pawnbb) & _emptySquares;
                    return single_push_b | (BoardUtils.MapSouth(single_push_b) & _emptySquares  & 0x000000FF00000000);
            }
            return 0UL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong PawnAttacksMoves(byte _square, byte _color)
        {
            return pawn_attacks[(_color * 64) + _square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BishopMoves(in ulong _occ , byte _sq)
        {
            MagicEntry entry = MAGICS_BISHOP[_sq];
            return sliding_attacks[GetMagicIndex(in _occ, 64-9, in entry)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RookMoves(in ulong _occ , byte _sq)
        {
            MagicEntry entry = MAGICS_ROOK[((byte)_sq)];
            return sliding_attacks[GetMagicIndex(in _occ, 64-12, in entry)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong QueenMoves(in ulong _occ, byte _sq)
        {
            return RookMoves(_occ, _sq) | BishopMoves(_occ, _sq);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong KnightMoves(byte _sq)
        {
            return knight_attacks[(byte)_sq];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong KingMoves(byte _sq)
        {
            return king_attacks[(byte)_sq];
        }

        // Patterns 

        public static ulong PatternSliding(byte _piece, byte _sq)
        {
            ulong result = 0UL;
            int rk = _sq / 8, fl = _sq % 8, r, f;
            if (_piece == Piece.Bishop)
            {
                for(r=rk+1, f=fl+1; r<=6 && f<=6; r++, f++) result |= (1UL << (f + r*8));
                for(r=rk+1, f=fl-1; r<=6 && f>=1; r++, f--) result |= (1UL << (f + r*8));
                for(r=rk-1, f=fl+1; r>=1 && f<=6; r--, f++) result |= (1UL << (f + r*8));
                for(r=rk-1, f=fl-1; r>=1 && f>=1; r--, f--) result |= (1UL << (f + r*8));
            }else if (_piece == Piece.Rook)
            {
                for(r = rk+1; r <= 6; r++) result |= (1UL << (fl + r*8));
                for(r = rk-1; r >= 1; r--) result |= (1UL << (fl + r*8));
                for(f = fl+1; f <= 6; f++) result |= (1UL << (f + rk*8));
                for(f = fl-1; f >= 1; f--) result |= (1UL << (f + rk*8));
            }
            return result;
        }

        public static ulong PatternSlidingRelevant(byte _piece, byte _sq, ulong _mask)
        {
            ulong attacks = 0UL;
            byte square;
            int rank_origin = _sq / 8;
            int file_origin = _sq % 8;
            if (_piece == Piece.Bishop)
            {
                // bottom-left
                for (int rank = rank_origin + 1,file = file_origin - 1; rank < 8 && file >= 0; rank++ , file--)
                {
                    square = (byte)(file + rank * 8);
                    attacks |= 1UL << square;
                    if (BitUtils.Contains(_mask , square)) break;
                }
                // bottom-right
                for (int rank = rank_origin + 1,file = file_origin + 1; rank < 8 && file < 8; rank++ , file++)
                {
                    square = (byte)(file + rank * 8);
                    attacks |= 1UL << square;
                    if (BitUtils.Contains(_mask , square)) break;
                }
                // top-left
                for (int rank = rank_origin - 1,file = file_origin - 1; rank >= 0 && file >= 0; rank-- , file--) 
                {
                    square = (byte)(file + rank * 8);
                    attacks |= 1UL << square;
                    if(BitUtils.Contains(_mask , square)) break;
                }
                // top-right 
                for (int rank = rank_origin - 1,file = file_origin + 1; rank >= 0 && file < 8; rank-- , file++) 
                {
                    square = (byte)(file + rank * 8);
                    attacks |= 1UL << square;
                    if (BitUtils.Contains(_mask , square)) break;
                }
            }else if (_piece == Piece.Rook)
            {
                // top
                for (int rank = rank_origin + 1 ; rank < 8; rank++ )
                {
                    square = (byte)(file_origin + rank * 8);
                    attacks |= 1UL << square;
                    if (BitUtils.Contains(_mask , square)) break;
                } 
                // bottom
                for (int rank = rank_origin - 1 ; rank >= 0; rank-- )
                {
                    square = (byte)(file_origin + rank * 8);
                    attacks |= 1UL << square;
                    if (BitUtils.Contains(_mask , square)) break;
                } 
                 // left
                for (int file = file_origin - 1 ; file >= 0; file-- )
                {
                    square = (byte)(file + rank_origin * 8);
                    attacks |= 1UL << square;
                    if (BitUtils.Contains(_mask , square)) break;
                }
                // right
                for (int file = file_origin + 1 ; file < 8; file++ ) 
                {
                    square = (byte)(file + rank_origin * 8);
                    attacks |= 1UL << square;
                    if(BitUtils.Contains(_mask , square)) break;
                }
            }
            return attacks;
        }

        public static ulong PatternKnight(byte _sq)
        {
            ulong moves = 0ul;
            ulong bb = 1UL << ((byte)_sq);
            
            // bottom moves
            moves |= ((bb << 17) & BoardUtils.MASK_PATTERN_FILE_A);
            moves |= ((bb << 15) & BoardUtils.MASK_PATTERN_FILE_H);
            moves |= ((bb << 10) & BoardUtils.MASK_PATTERN_FILE_AB);
            moves |= ((bb << 6)  & BoardUtils.MASK_PATTERN_FILE_GH);

            // top moves
            moves |= ((bb >> 17) & BoardUtils.MASK_PATTERN_FILE_H);
            moves |= ((bb >> 15) & BoardUtils.MASK_PATTERN_FILE_A);
            moves |= ((bb >> 10) & BoardUtils.MASK_PATTERN_FILE_GH);
            moves |= ((bb >> 6)  & BoardUtils.MASK_PATTERN_FILE_AB);

            return moves;
        }

        public static ulong PatternKing(byte _sq)
        {
            ulong moves = 0ul;
            ulong bb = 1UL << ((byte)_sq);

            moves |= ((bb << 7) & BoardUtils.MASK_PATTERN_FILE_H);
            moves |= (bb << 8);
            moves |= ((bb << 9) & BoardUtils.MASK_PATTERN_FILE_A);
            moves |= ((bb << 1) & BoardUtils.MASK_PATTERN_FILE_A);

            moves |= ((bb >> 7) & BoardUtils.MASK_PATTERN_FILE_A);
            moves |= (bb  >> 8);
            moves |= ((bb >> 9) & BoardUtils.MASK_PATTERN_FILE_H);
            moves |= ((bb >> 1) & BoardUtils.MASK_PATTERN_FILE_H);
            return moves;
        }
        
        public static ulong PatternPawnAttacks(byte _sq, byte _color)
        {
            ulong bb = 1UL << (byte)_sq;
            ulong moves = 0UL;

            switch(_color)
            {
                case Piece.White:
                    moves |= ((bb << 7) & BoardUtils.MASK_PATTERN_FILE_H);
                    moves |= ((bb << 9) & BoardUtils.MASK_PATTERN_FILE_A);
                break;

                case Piece.Black: 
                    moves |= ((bb >> 7) & BoardUtils.MASK_PATTERN_FILE_A);
                    moves |= ((bb >> 9) & BoardUtils.MASK_PATTERN_FILE_H);
                break;
            }

            return moves;
        }

        public static ulong FileRankMask(int _file, int _rank)
		{
			ulong result = 0ul;
			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					int square = rank * 8 + file;
					if (_file != -1 && file == _file)
					{
						result |= (1ul << square);
					}
					else if (_rank != -1 && rank == _rank)
					{
						result |= (1ul << square);
					}
				}
			}
			return result;
		}

        public static ulong MaskVariation(int _index , int _count, ulong _mask)
        {
            int i, sq;
            ulong result = 0UL;
            for(i = 0; i < _count; i++) 
            {
                sq = BitUtils.BitScan(_mask);
                if((_index & (1 << i)) != 0) result |= (1UL << sq);

                _mask = BitUtils.PopLsb(_mask);
            }
            return result;
        }
    }
}