using System.Runtime.CompilerServices;

namespace SnailChess.Core
{
    // Move is encoded in 16 bits
    public readonly struct Move 
    {
        public static readonly Move NO_MOVE = 0;
        public const ushort MAX_MOVES = 256;
        
        private readonly ushort data;
        
        public Move (byte _from , byte _to , MoveFlag _flag) => data = (ushort)(_from | (_to << 6) | ((byte)_flag << 12));
        public Move(BoardSquare _from, BoardSquare _to, MoveFlag _flag) : this((byte)_from, (byte)_to, _flag){ }
        
        private Move(ushort _rawData)
        {
            data = _rawData;
        }
        
        public byte from 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                return (byte)(data & 0x3f);
            }
        }

        public byte to
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                return (byte)((data >> 6) & 0x3f);
            }
        }
        
        public MoveFlag flag 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                return (MoveFlag)(data >> 12);
            }
        }

        public byte promotionType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                switch(flag & ~MoveFlag.Capture)
                {
                    case MoveFlag.Promotion_Queen:  return Piece.Queen;
                    case MoveFlag.Promotion_Rook:   return Piece.Rook;
                    case MoveFlag.Promotion_Bishop: return Piece.Bishop;
                    case MoveFlag.Promotion_Knight: return Piece.Knight;
                    default: return Piece.None;
                }
            }
        }
        
        public bool IsPromotion
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                return (data >> 12 & (byte)MoveFlag.Promotion_Knight) != 0;
            }
        }
        
        public bool IsCapture
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                return (data >> 12 & (byte)MoveFlag.Capture) != 0;
            }
        }

        public bool IsCastle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                switch(flag)
                {
                    case MoveFlag.CastleKing:
                    case MoveFlag.CastleQueen:
                        return true;

                    default:
                        return false;
                }
            }
        }
        
        public bool IsQuiet 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                switch(flag)
                {
                    case MoveFlag.Quiet:
                    case MoveFlag.PawnDoublePush:
                    case MoveFlag.CastleKing:
                    case MoveFlag.CastleQueen:
                        return true;
                    
                    default:
                        return false;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(Move _m)
        {
            return _m.data != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Move(ushort _val)
        {
            return new Move(_val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ushort(Move _move)
        {
            return _move.data;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Move _left , Move _right)
        {
            return _left.data == _right.data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Move _left , Move _right)
        {
            return _left.data != _right.data;
        }


		public override string ToString()
		{
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append($"{(BoardSquare)from}{(BoardSquare)to}");
            
            if (IsPromotion)
            {   
                switch(promotionType)
                {
                    case Piece.Knight:
                    builder.Append("n");
                    break;
                    case Piece.Bishop:
                    builder.Append("b");
                    break;
                    case Piece.Rook:
                    builder.Append("r");
                    break;
                    case Piece.Queen:
                    builder.Append("q");
                    break;
                }
            }

            return builder.ToString();
		}

		public override bool Equals(object obj)
		{
			return data.Equals(obj);
		}

		public override int GetHashCode()
		{
			return data.GetHashCode();
		}

        public static Move FromLAN(string _moveStr, in ChessBoard _board)
        {
            Move[] legal_moves = GetLegalMoves(_board.ExportPosition());
            return FromLAN(_moveStr, in legal_moves);
        }

        public static Move FromLAN(string _moveStr , in Move[] _legal_moves)
        {   
            if (_legal_moves.Length > 0)
            {
                for (int i = 0; i < _legal_moves.Length; i++)
                {
                    if (_legal_moves[i].ToString() == _moveStr)
                    {
                        return _legal_moves[i];
                    }
                }
            }
            
            return NO_MOVE;
        }
        
        public static Move[] GetLegalMoves(in BoardPosition _position)
        {
            var valid_moves = new System.Collections.Generic.List<Move>();
            ChessBoard dummyBoard = new ChessBoard(_position);

            System.Span<Move> moves = stackalloc Move[MAX_MOVES];
            Move current_move;

            short target_index = dummyBoard.GenerateMoves(in moves);
            for (short i = 0; i <= target_index; i++)
            {
                current_move = moves[i];
                if (dummyBoard.MakeMove(in current_move))
                {
                    valid_moves.Add(current_move);
                    dummyBoard.UndoLastMove();
                }
            }

            return valid_moves.ToArray();
        }
	}
}