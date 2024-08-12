using System.Runtime.CompilerServices;

namespace SnailChess.Core
{
    public static class RandomBits
    {
        private const int SEED = 1804289383;
        private static uint seed = SEED;

        static RandomBits()
        {
            ResetSeed();
        }
        
        public static void ResetSeed()
        {
            seed = SEED;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Random()
        {
            uint x = seed;
            x ^= x << 13;
	        x ^= x << 17;
	        x ^= x >> 5;
            seed = x;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Random64()
        {
            ulong n1,n2,n3,n4;
            n1 = (ulong)Random() & 0xFFFF;
            n2 = (ulong)Random() & 0xFFFF;
            n3 = (ulong)Random() & 0xFFFF;
            n4 = (ulong)Random() & 0xFFFF;
            return n1 | (n2 << 16) | (n3 << 32) | (n4 << 48);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RandomMagic()
        {
            return Random64() & Random64() & Random64();
        }
    }
}