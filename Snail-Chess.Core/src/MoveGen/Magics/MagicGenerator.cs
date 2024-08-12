using System.Runtime.CompilerServices;

namespace SnailChess.Core.MoveGen.Magics
{
    public static class MagicGenerator
    {        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetRawMagicIndex(in ulong _occ, in ulong _mask, in ulong _magic , int _bits)
        {
            return (int)(((_occ & _mask) * _magic) >> 64 - _bits);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MagicEntry FindMagic(byte _sq, byte _piece)
        {
            if (_piece != Piece.Bishop && _piece != Piece.Rook) return MagicEntry.EMPTY;
            byte sq = (byte)_sq;
            uint offset = (uint)(_piece == Piece.Rook ? (0x8000 + (sq * 4096)) : sq * 512);
            byte bits = (byte)(_piece == Piece.Bishop ? 9 : 12);
            ulong mask, magic;
            ulong[] b = new ulong[4096], a = new ulong[4096], used = new ulong[4096];
            int i, j, k, n, fail;

            mask = MoveGenerator.PatternSliding(_piece, _sq);
            n = BitUtils.Count(mask);
            for (i = 0; i < (1 << n); i++)
            {
                b[i] = MoveGenerator.MaskVariation(i, n, mask);
                a[i] = MoveGenerator.PatternSlidingRelevant(_piece, _sq, b[i]);
            }
            
            for (k = 0; k < 200000000; k++)
            {
                magic = RandomBits.RandomMagic();
                if (BitUtils.Count((mask * magic) & 0xFF00000000000000UL) < 6) continue;
                for (i = 0; i < 4096; i++) used[i] = 0UL;
                for (i = 0, fail = 0; fail == 0 && i < (1 << n); i++)
                {
                    j = GetRawMagicIndex(in b[i], in mask, in magic, bits);
                    if (used[j] == 0UL) used[j] = a[i];
                    else if (used[j] != a[i])
                    {
                        fail = 1;
                    };
                }
                
                if (fail == 0)
                {
                    return new MagicEntry(mask, magic, offset);
                }
            }
            return MagicEntry.EMPTY;
        }

        public static MagicEntry[] FindMagics(byte _piece)
        {
            MagicEntry[] result = new MagicEntry[64];
            if (_piece != Piece.Bishop && _piece != Piece.Rook) return result;

            for (byte sq = 0; sq < result.Length; sq++)
            {
                result[sq] = FindMagic(sq, _piece);
            }
            return result;
        }
    }
}