using System.Runtime.CompilerServices;

namespace SnailChess.Core
{
    public static class Piece 
    {
        public const byte Black  = 0;
        public const byte White  = 1;
        public const byte Pawn   = 2;
        public const byte Knight = 3;
        public const byte Bishop = 4;
        public const byte Rook   = 5;
        public const byte Queen  = 6;
        public const byte King   = 7;
        public const byte None   = 8;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte FlipColor(byte _color) => (byte)(_color ^ 1);
    }
}