using System.Runtime.CompilerServices;

namespace SnailChess.Core
{
    public static class BitUtils
    {
        public static void Init() { }
        private static readonly byte[] Index64 =
        {
            0, 47,  1, 56, 48, 27,  2, 60,
            57, 49, 41, 37, 28, 16,  3, 61,
            54, 58, 35, 52, 50, 42, 21, 44,
            38, 32, 29, 23, 17, 11,  4, 62,
            46, 55, 26, 59, 40, 36, 15, 53,
            34, 51, 20, 43, 31, 22, 10, 45,
            25, 39, 14, 33, 19, 30,  9, 24,
            13, 18,  8, 12,  7,  6,  5, 63
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Count(ulong _value)
        {
            _value = _value - ((_value >> 1) & 0x5555555555555555);
            _value = (_value & 0x3333333333333333) + ((_value >> 2) & 0x3333333333333333);
            return (byte)((((_value + (_value >> 4)) & 0x0F0F0F0F0F0F0F0F) * 0x0101010101010101) >> 56);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Reverse(ulong _value)
        {
			_value = ((_value >> 1) & 0x5555555555555555ul)  | ((_value & 0x5555555555555555ul) << 1);
            _value = ((_value >> 2) & 0x3333333333333333ul)  | ((_value & 0x3333333333333333ul) << 2);
            _value = ((_value >> 4) & 0x0F0F0F0F0F0F0F0Ful)  | ((_value & 0x0F0F0F0F0F0F0F0Ful) << 4);
            _value = ((_value >> 8) & 0x00FF00FF00FF00FFul)  | ((_value & 0x00FF00FF00FF00FFul) << 8);
            _value = ((_value >> 16) & 0x0000FFFF0000FFFFul) | ((_value & 0x0000FFFF0000FFFFul) << 16);
            return (_value >> 32) | (_value << 32);
		}
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte BitScan(ulong _value) => Index64[((_value ^ (_value - 1)) * 0x03f79d71b4cb0a89) >> 58];
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(ulong _value, byte _index) => (1 & (_value >> _index)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong PopLsb(ulong _value) => _value & (_value - 1);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetLsb(ulong _value) => _value & (0 - _value);
    }
}