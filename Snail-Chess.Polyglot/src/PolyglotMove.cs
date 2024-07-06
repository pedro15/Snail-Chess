using SnailChess.Core;

namespace SnailChess.Polyglot
{
    public struct PolyglotMove 
    {
        /* 
            -------------------------------------------
            Polyglot move format is encoded in 15 bits:
            -------------------------------------------
            bits                    -  meaning
            ===========================================|
            |   000 000 000 000 111 - to file          |
            |   000 000 000 111 000 - to row           |
            |   000 000 111 000 000 - from file        |
            |   000 111 000 000 000 - from row         |
            |   111 000 000 000 000 - promotion        |
        */

        #pragma warning disable 649
        private ushort move_data;
        #pragma warning restore 649

		public byte to_file => (byte)(move_data & 0x7);
        public byte to_rank => (byte)((move_data >> 3) & 0x7);        
        public byte to_square => (byte)(8 * to_rank + to_file);

        public byte from_file => (byte)((move_data >> 6) & 0x7);
        public byte from_rank  => (byte)((move_data >> 9) & 0x7);
        public byte from_square => (byte)(8 * from_rank + from_file);


        public byte promotion => (byte)((move_data >> 12) & 0x7);
        
        public static implicit operator bool(PolyglotMove _move)
        {
            return _move.move_data != 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj,null)) return false;
            if (obj is PolyglotMove move)  return move.move_data == move_data;
            return false;
        }
        
        public override int GetHashCode()
        {
            return move_data.GetHashCode();
        }
        
        public string ToLAN()
        {
            BoardSquare sq_origin = (BoardSquare)from_square;
            BoardSquare sq_target = (BoardSquare)to_square;
            string result = $"{sq_origin}{sq_target}";

            switch((PolyglotPromotion)promotion)
            {
                case PolyglotPromotion.knight:
                result += "n";
                break;

                case PolyglotPromotion.bishop:
                result += "b";
                break;

                case PolyglotPromotion.rook:
                result += "r";
                break;

                case PolyglotPromotion.queen:
                result += "q";
                break;
            }

            return result;
        }
    }
}