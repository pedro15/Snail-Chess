using System;
using System.Runtime.CompilerServices;
using SnailChess.Core.MoveGen;
using SnailChess.Core.Hashing;

namespace SnailChess.Core 
{
    public sealed class ChessBoard : IDisposable
    {
        public const int HISTORY_LENGHT = 2048;
        private const byte HALF_MOVES_LIMIT = 128;

        // Current board state data
        public readonly ulong[] bitboards;
        public ulong key;
        public uint key_pawns;
        public byte sideToMove;
        public CastleRights castleRights;
        public byte ep_square;
        public byte halfMoves;
        public byte fullMoves;
        
        public ulong Occupancy 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                return bitboards[Piece.White] | bitboards[Piece.Black];
            }
        }

        

        // State handling
        private readonly BoardPosition[] states;
        private short state_index = -1;
        public ChessBoard() : this(Notation.ParseFEN(Notation.POSITION_DEFAULT)) { }
        public ChessBoard(string _fen) : this(Notation.ParseFEN(_fen)) { }
        public ChessBoard(BoardPosition _position)
        {
            bitboards = new ulong[8];
            states = new BoardPosition[HISTORY_LENGHT];
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = BoardPosition.EmptyPosition();
            }

            LoadPosition_Internal(_position);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LoadPosition(in BoardPosition _position)
        {
            ClearPosition();
            LoadPosition_Internal(in _position);
        }

        public BoardPosition ExportPosition()
        {
            BoardPosition pos = BoardPosition.EmptyPosition();
            Array.Copy(bitboards, pos.bitboards, 8);
            pos.key = key;
            pos.key_pawns = key_pawns;
            pos.sideToMove = sideToMove;
            pos.castleRights = castleRights;
            pos.ep_square = ep_square;
            pos.halfmoves = halfMoves;
            pos.fullmoves = fullMoves;
            return pos;
        }

        private void ClearPosition()
        {
            BoardPosition current_state;
            for (int i = 0; i < states.Length; i++)
            {
                current_state = states[i];
                Array.Clear(current_state.bitboards, 0, 8);
                current_state.sideToMove = Piece.White;
                current_state.key = 0UL;
                current_state.key_pawns = 0;
                current_state.castleRights = CastleRights.None;
                current_state.ep_square = 0;
                current_state.halfmoves = 0;
                current_state.fullmoves = 0;
                
                states[i] = current_state;
            }

            Array.Clear(bitboards, 0, 8);
            sideToMove = Piece.White;
            key = 0UL;
            key_pawns = 0;
            castleRights = CastleRights.None;
            ep_square = 0;
            halfMoves = 0;
            fullMoves = 0;
            state_index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LoadPosition_Internal(in BoardPosition _position)
        {
            key = _position.key;
            key_pawns = _position.key_pawns;
            sideToMove = _position.sideToMove;
            castleRights = _position.castleRights;
            ep_square = _position.ep_square;
            halfMoves = _position.halfmoves;
            fullMoves = _position.fullmoves;
            Array.Copy(_position.bitboards, bitboards, 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CopyBoardState()
        {
            state_index++;
            BoardPosition pos = states[state_index];
            
            pos.castleRights = castleRights;
            pos.ep_square = ep_square;
            pos.sideToMove = sideToMove;
            pos.key = key;
            pos.key_pawns = key_pawns;
            pos.halfmoves = halfMoves;
            pos.fullmoves = fullMoves;
            Array.Copy(bitboards, pos.bitboards, 8);

            states[state_index] = pos;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GenerateMoves(in System.Span<Move> _move_list, bool _generateQuiets = true)
        {
            short move_index = -1;
            
            MoveFlag flag;
            ulong pieces_bb = 0UL, moves_bb = 0UL, 
                stm_bb = bitboards[sideToMove], opponent_bb = bitboards[Piece.FlipColor(sideToMove)], occ_bb = Occupancy;

            byte square_from, square_to;

            // ---- ( KING MOVES ) -----------------------------
            pieces_bb = bitboards[Piece.King] & stm_bb; 
            byte num_checks = 0;

            while(pieces_bb != 0)
            {
                square_from = BitUtils.BitScan(pieces_bb);
                pieces_bb = BitUtils.PopLsb(pieces_bb);
                
                num_checks = BitUtils.Count(AttackersTo(square_from, Piece.FlipColor(sideToMove)));
                moves_bb = MoveGenerator.KingMoves(square_from) & ~stm_bb;
                if (!_generateQuiets) moves_bb &= opponent_bb;

                while(moves_bb != 0)
                {
                    square_to = BitUtils.BitScan(moves_bb);
                    moves_bb = BitUtils.PopLsb(moves_bb);

                    flag = BitUtils.Contains(opponent_bb, square_to) ? MoveFlag.Capture : MoveFlag.Quiet;
                    _move_list[++move_index] = new Move(square_from, square_to, flag);
                }

                if (_generateQuiets && num_checks == 0)
                {
                    // Castle moves
                    if (sideToMove == Piece.Black)
                    {
                        // Black kingSide
                        if ((castleRights & CastleRights.KingSide_Black) != 0 && (BoardUtils.MASK_CASTLE_KINGSIDE_BLACK & ~occ_bb) == BoardUtils.MASK_CASTLE_KINGSIDE_BLACK)
                        {
                            square_to = (byte)BoardSquare.g8;
                            _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.CastleKing);
                        }
                        // Black QueenSide
                        if ((castleRights & CastleRights.QueenSide_Black) != 0 && (BoardUtils.MASK_CASTLE_QUEENSIDE_BLACK & ~occ_bb) == BoardUtils.MASK_CASTLE_QUEENSIDE_BLACK)
                        {
                            square_to = (byte)BoardSquare.c8;
                            _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.CastleQueen);
                        }
                    }else 
                    {
                        // White KingSide
                        if ((castleRights & CastleRights.KingSide_White) != 0 && (BoardUtils.MASK_CASTLE_KINGSIDE_WHITE & ~occ_bb) == BoardUtils.MASK_CASTLE_KINGSIDE_WHITE)
                        {
                            square_to = (byte)BoardSquare.g1;
                            _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.CastleKing);
                        }

                        // White QueenSide
                        if ((castleRights & CastleRights.QueenSide_White) != 0 && (BoardUtils.MASK_CASTLE_QUEENSIDE_WHITE & ~occ_bb) == BoardUtils.MASK_CASTLE_QUEENSIDE_WHITE)
                        {
                            square_to = (byte)BoardSquare.c1;
                            _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.CastleQueen);
                        }
                    }
                }
            }

            // we're on double check, we can return early
            if (num_checks >= 2) return move_index;

            // ---- ( PAWN MOVES ) -----------------------------
            pieces_bb = bitboards[Piece.Pawn] & stm_bb;

            while(pieces_bb != 0)
            {
                square_from = BitUtils.BitScan(pieces_bb);
                pieces_bb = BitUtils.PopLsb(pieces_bb);

                moves_bb = MoveGenerator.PawnAttacksMoves(square_from, sideToMove) & opponent_bb;
                
                // Captures
                while(moves_bb != 0)
                {
                    square_to = BitUtils.BitScan(moves_bb);
                    moves_bb = BitUtils.PopLsb(moves_bb);
                    flag = MoveFlag.Capture;
                    
                    if (BitUtils.Contains(BoardUtils.PROMOTION_AREA[sideToMove], square_to))
                    {
                        // Captures Promotion moves
                        // BISHOP
                        _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.Capture_Promotion_Bishop);
                        // ROOK
                        _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.Capture_Promotion_Rook);
                        // KNIGHT 
                        _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.Capture_Promotion_Knight);
                        // QUEEN
                        _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.Capture_Promotion_Queen);
                    }else 
                    {
                        _move_list[++move_index] = new Move(square_from,square_to,flag);
                    }
                }

                // En-passant moves
                if (ep_square != 0)
                {
                    if (BitUtils.Contains(MoveGenerator.PawnAttacksMoves(square_from, sideToMove), ep_square))
                    {
                        _move_list[++move_index] = new Move(square_from, ep_square, MoveFlag.EnPassant);
                    }
                }

                // Quiet moves
                if (_generateQuiets)
                {
                    moves_bb = MoveGenerator.PawnQuietMoves((1UL << square_from) , ~occ_bb, sideToMove);
                    while (moves_bb != 0)
                    {
                        square_to = BitUtils.BitScan(moves_bb);
                        moves_bb = BitUtils.PopLsb(moves_bb);

                        if (BoardUtils.DISTANCE[square_from][square_to] > 1)
                            flag = MoveFlag.PawnDoublePush; 
                        else 
                            flag = MoveFlag.Quiet;
                        
                        if (flag == MoveFlag.Quiet && BitUtils.Contains(BoardUtils.PROMOTION_AREA[sideToMove], square_to))
                        {
                            // Quiet Promotion moves
                            // BISHOP
                            _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.Promotion_Bishop);
                            // ROOK
                            _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.Promotion_Rook);
                            // KNIGHT 
                            _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.Promotion_Knight);
                            // QUEEN
                            _move_list[++move_index] = new Move(square_from, square_to, MoveFlag.Promotion_Queen);
                        }else 
                        {
                            _move_list[++move_index] = new Move(square_from,square_to, flag);
                        }
                    }
                }
            }

            // ---- ( KNIGHT MOVES ) -----------------------------
            pieces_bb = bitboards[Piece.Knight] & stm_bb;

            while(pieces_bb != 0)
            {
                square_from = BitUtils.BitScan(pieces_bb);
                pieces_bb = BitUtils.PopLsb(pieces_bb);

                moves_bb = MoveGenerator.KnightMoves(square_from) & ~stm_bb;
                if (!_generateQuiets) moves_bb &= opponent_bb;

                while(moves_bb != 0)
                {
                    square_to = BitUtils.BitScan(moves_bb);
                    moves_bb = BitUtils.PopLsb(moves_bb);

                    flag = BitUtils.Contains(opponent_bb, square_to) ? MoveFlag.Capture : MoveFlag.Quiet;
                    _move_list[++move_index] = new Move(square_from,square_to,flag);
                }
            }

            // ---- ( ROOK MOVES ) -----------------------------
            pieces_bb = bitboards[Piece.Rook] & stm_bb;

            while(pieces_bb != 0)
            {
                square_from = BitUtils.BitScan(pieces_bb);
                pieces_bb = BitUtils.PopLsb(pieces_bb);

                moves_bb = MoveGenerator.RookMoves(occ_bb, square_from) & ~stm_bb;
                if (!_generateQuiets) moves_bb &= opponent_bb;

                while(moves_bb != 0)
                {
                    square_to = BitUtils.BitScan(moves_bb);
                    moves_bb = BitUtils.PopLsb(moves_bb);

                    flag = BitUtils.Contains(opponent_bb, square_to) ? MoveFlag.Capture : MoveFlag.Quiet;
                    _move_list[++move_index] = new Move(square_from, square_to, flag);
                }
            }

            // ---- ( BISHOP MOVES ) -----------------------------
            pieces_bb = bitboards[Piece.Bishop] & stm_bb;

            while(pieces_bb != 0)
            {
                square_from = BitUtils.BitScan(pieces_bb);
                pieces_bb = BitUtils.PopLsb(pieces_bb);

                moves_bb = MoveGenerator.BishopMoves(occ_bb, square_from) &~ stm_bb;
                if (!_generateQuiets) moves_bb &= opponent_bb;

                while(moves_bb != 0)
                {
                    square_to = BitUtils.BitScan(moves_bb);
                    moves_bb = BitUtils.PopLsb(moves_bb);

                    flag = BitUtils.Contains(opponent_bb, square_to) ? MoveFlag.Capture : MoveFlag.Quiet;
                    _move_list[++move_index] = new Move(square_from, square_to, flag);
                }
            }

            // ---- ( QUEEN MOVES ) -----------------------------
            pieces_bb = bitboards[Piece.Queen] & stm_bb;

            while(pieces_bb != 0)
            {
                square_from = BitUtils.BitScan(pieces_bb);
                pieces_bb = BitUtils.PopLsb(pieces_bb);

                moves_bb = MoveGenerator.QueenMoves(occ_bb, square_from) &~ stm_bb;
                if (!_generateQuiets) moves_bb &= opponent_bb;

                while(moves_bb != 0)
                {
                    square_to = BitUtils.BitScan(moves_bb);
                    moves_bb = BitUtils.PopLsb(moves_bb);

                    flag = BitUtils.Contains(opponent_bb, square_to) ? MoveFlag.Capture : MoveFlag.Quiet;
                    _move_list[++move_index] = new Move(square_from, square_to, flag);
                }
            }
            
            return move_index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MakeMove(in Move _move)
        {
            if (!_move) return false;
            CopyBoardState();
            
            if(halfMoves < HALF_MOVES_LIMIT) halfMoves++;
            MoveFlag flag = _move.flag;
            ulong mask_from = 1UL << _move.from, mask_to = 1UL << _move.to, mask_helper;
            byte piece_moved = PieceOn(_move.from), piece_captured = (flag != MoveFlag.EnPassant && _move.IsCapture) ? PieceOn(_move.to) : Piece.None;
            byte color_opponent = Piece.FlipColor(sideToMove);

            // Captures
            if (_move.IsCapture)
            {
                halfMoves = 0;
                if (piece_captured != Piece.None)
                {
                    // normal captures
                    bitboards[piece_captured] &= ~mask_to;
                    bitboards[color_opponent] &= ~mask_to;
                    key ^= BoardHash.piecesHashKeys[_move.to][piece_captured][color_opponent];

                    if (piece_captured == Piece.Rook)
                    {
                        key ^= BoardHash.castleHashKeys[(byte)castleRights];
                        castleRights &= ~BoardUtils.CASTLE_RIGHTS_ROOK[_move.to][color_opponent];
                        key ^= BoardHash.castleHashKeys[(byte)castleRights];
                    }else if (piece_captured == Piece.Pawn)
                    {
                        key_pawns ^= PawnHash.pawnHashKeys[_move.to][color_opponent];
                    }
                }else if (flag == MoveFlag.EnPassant && piece_moved == Piece.Pawn && ep_square != 0 && _move.to == ep_square && 
                    ((sideToMove == Piece.Black && BoardUtils.SquareToRank(ep_square) == 2) || BoardUtils.SquareToRank(ep_square) == 5 ))
                {
                    // en-passant captures
                    byte target_sq = (sideToMove == Piece.Black) ? (byte)(ep_square + 8) : (byte)(ep_square - 8);
                    key ^= BoardHash.piecesHashKeys[target_sq][Piece.Pawn][color_opponent];
                    key_pawns ^= PawnHash.pawnHashKeys[target_sq][color_opponent];

                    // remove the captured pawn from the bitboards
                    mask_helper = 1UL << target_sq;
                    bitboards[color_opponent] &= ~mask_helper;
                    bitboards[Piece.Pawn] &= ~mask_helper;
                }else
                {
                    UndoLastMove();
                    return false;
                }
            }

            // remove our piece from source square
            bitboards[sideToMove]  &= ~mask_from;
            bitboards[piece_moved] &= ~mask_from;
            key ^= BoardHash.piecesHashKeys[_move.from][piece_moved][sideToMove];

            // place our piece to target square
            bitboards[sideToMove]  |= mask_to;
            bitboards[piece_moved] |= mask_to;
            key ^= BoardHash.piecesHashKeys[_move.to][piece_moved][sideToMove];

            switch(piece_moved)
            {
                case Piece.Pawn:

                // Promotion
                if (_move.IsPromotion && BitUtils.Contains(BoardUtils.PROMOTION_AREA[sideToMove], _move.to))
                {
                    key ^= BoardHash.piecesHashKeys[_move.to][Piece.Pawn][sideToMove];
                    bitboards[Piece.Pawn] &= ~mask_to;

                    bitboards[_move.promotionType] |= mask_to;
                    key ^= BoardHash.piecesHashKeys[_move.to][_move.promotionType][sideToMove];
                    key_pawns ^= PawnHash.pawnHashKeys[_move.from][sideToMove];
                }else
                {
                    key_pawns ^= PawnHash.pawnHashKeys[_move.from][sideToMove];
                    key_pawns ^= PawnHash.pawnHashKeys[_move.to][sideToMove];
                }
                
                halfMoves = 0;

                break;

                case Piece.King:
                // Castle
                if (flag == MoveFlag.CastleKing)
                {
                    // Kingside
                    if (sideToMove == Piece.White && (castleRights & CastleRights.KingSide_White) != 0 &&
                        !IsAttacked((byte)BoardSquare.f1, color_opponent) && !IsAttacked((byte)BoardSquare.g1, color_opponent))
                    {
                        mask_helper = 0x80;
                        bitboards[Piece.Rook] &= ~mask_helper;
                        bitboards[Piece.White] &= ~mask_helper;
                        key ^= BoardHash.piecesHashKeys[(byte)BoardSquare.h1][Piece.Rook][Piece.White];

                        mask_helper = 0x20;
                        bitboards[Piece.Rook] |= mask_helper;
                        bitboards[Piece.White] |= mask_helper;
                        key ^= BoardHash.piecesHashKeys[(byte)BoardSquare.f1][Piece.Rook][Piece.White];

                    }else if (sideToMove == Piece.Black && (castleRights & CastleRights.KingSide_Black) != 0 &&
                        !IsAttacked((byte)BoardSquare.f8, color_opponent) && !IsAttacked((byte)BoardSquare.g8, color_opponent))
                    {
                        mask_helper = 0x8000000000000000;
                        bitboards[Piece.Rook] &= ~mask_helper;
                        bitboards[Piece.Black] &= ~mask_helper;
                        key ^= BoardHash.piecesHashKeys[(byte)BoardSquare.h8][Piece.Rook][Piece.Black];

                        mask_helper = 0x2000000000000000;
                        bitboards[Piece.Rook]  |= mask_helper;
                        bitboards[Piece.Black] |= mask_helper;
                        key ^= BoardHash.piecesHashKeys[(byte)BoardSquare.f8][Piece.Rook][Piece.Black];
                    }else 
                    {
                        UndoLastMove();
                        return false;
                    }
                }else if (flag == MoveFlag.CastleQueen)
                {
                    // QueenSide
                    if (sideToMove == Piece.White && (castleRights & CastleRights.QueenSide_White) != 0 && 
                        !IsAttacked((byte)BoardSquare.d1, color_opponent) && !IsAttacked((byte)BoardSquare.c1, color_opponent))
                    {
                        mask_helper = 0x1;
                        bitboards[Piece.Rook] &= ~mask_helper;
                        bitboards[Piece.White] &= ~mask_helper;
                        key ^= BoardHash.piecesHashKeys[(byte)BoardSquare.a1][Piece.Rook][Piece.White];
                        
                        mask_helper = 0x8;
                        bitboards[Piece.Rook] |= mask_helper;
                        bitboards[Piece.White] |= mask_helper;
                        key ^= BoardHash.piecesHashKeys[(byte)BoardSquare.d1][Piece.Rook][Piece.White];
                    }else if (sideToMove == Piece.Black && (castleRights & CastleRights.QueenSide_Black) != 0 &&
                        !IsAttacked((byte)BoardSquare.d8, color_opponent) && !IsAttacked((byte)BoardSquare.c8, color_opponent))
                    {
                        mask_helper = 0x100000000000000;
                        bitboards[Piece.Rook] &= ~mask_helper;
                        bitboards[Piece.Black] &= ~mask_helper;
                        key ^= BoardHash.piecesHashKeys[(byte)BoardSquare.a8][Piece.Rook][Piece.Black];

                        mask_helper = 0x800000000000000;
                        bitboards[Piece.Rook] |= mask_helper;
                        bitboards[Piece.Black] |= mask_helper;
                        key ^= BoardHash.piecesHashKeys[(byte)BoardSquare.d8][Piece.Rook][Piece.Black];
                    }else 
                    {
                        UndoLastMove();
                        return false;
                    }
                }

                // Remove castle flags for the respective side when the king is moved
                if (sideToMove == Piece.White && (castleRights & CastleRights.WhiteRights) != 0)
                {
                    key ^= BoardHash.castleHashKeys[(byte)castleRights];
                    castleRights &= ~CastleRights.WhiteRights;
                    key ^= BoardHash.castleHashKeys[(byte)castleRights];
                }else if (sideToMove == Piece.Black && (castleRights & CastleRights.BlackRights) != 0)
                {
                    key ^= BoardHash.castleHashKeys[(byte)castleRights];
                    castleRights &= ~CastleRights.BlackRights;
                    key ^= BoardHash.castleHashKeys[(byte)castleRights];
                }
                break;

                case Piece.Rook:
                key ^= BoardHash.castleHashKeys[(byte)castleRights];
                castleRights &= ~BoardUtils.CASTLE_RIGHTS_ROOK[_move.from][sideToMove];
                key ^= BoardHash.castleHashKeys[(byte)castleRights];
                break;
            }

            // En-passant flag
            if (flag == MoveFlag.PawnDoublePush && piece_moved == Piece.Pawn)
            {
                key ^= BoardHash.epHashKeys[BoardUtils.SquareToFile(ep_square)];
                ep_square = sideToMove == Piece.White ? (byte)(_move.to - 8) : (byte)(_move.to + 8);
                key ^= BoardHash.epHashKeys[BoardUtils.SquareToFile(ep_square)];
            }else if (ep_square != 0) 
            {
                key ^= BoardHash.epHashKeys[BoardUtils.SquareToFile(ep_square)];
                ep_square = 0;
            }

            if (IsKingChecked())
            {
                UndoLastMove();
                return false;
            }
            
            if (sideToMove == Piece.Black)
                fullMoves++;

            key ^= BoardHash.sideToMoveHashKeys[sideToMove];
            sideToMove ^= 1;
            key ^= BoardHash.sideToMoveHashKeys[sideToMove];

            return true;   
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MakeNullMove()
        {
            // preserve board state
            CopyBoardState();
            // swap color
            key ^= BoardHash.sideToMoveHashKeys[sideToMove];
            sideToMove ^= 1;
            key ^= BoardHash.sideToMoveHashKeys[sideToMove];
            // remove en-pasant (if any)
            if (ep_square != 0)
            {
                key ^= BoardHash.epHashKeys[BoardUtils.SquareToFile(ep_square)];
                ep_square = 0;
            }
            // increase half-moves
            if (halfMoves < HALF_MOVES_LIMIT) halfMoves++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UndoLastMove()
        {
            if (state_index >= 0)
            {
                LoadPosition_Internal(in states[state_index--]);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetKingAttackers(byte _color)
        {
            byte king_sq = BitUtils.BitScan(bitboards[_color] & bitboards[Piece.King]);
            return AttackersTo(king_sq, Piece.FlipColor(_color));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKingChecked()
        {
            return IsKingChecked(sideToMove);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKingChecked(byte _color)
        {
            byte king_sq = BitUtils.BitScan(bitboards[_color] & bitboards[Piece.King]);
            return IsAttacked(king_sq, Piece.FlipColor(_color));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAttacked(byte _sq, byte _byColor)
        {
            return IsAttacked(_sq, _byColor, Occupancy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAttacked (byte _sq , byte _byColor, ulong _occ)
        {
            ulong color_bb = bitboards[_byColor];
            return  ((MoveGenerator.PawnAttacksMoves(_sq, (byte)(_byColor ^ 1)) & bitboards[Piece.Pawn] & color_bb) != 0) || 
                    ((MoveGenerator.KnightMoves(_sq) & bitboards[Piece.Knight] & color_bb ) != 0) || 
                    ((MoveGenerator.BishopMoves(in _occ, _sq) & (bitboards[Piece.Bishop] | bitboards[Piece.Queen]) & color_bb) != 0) || 
                    ((MoveGenerator.RookMoves(in _occ, _sq) & (bitboards[Piece.Rook] | bitboards[Piece.Queen]) & color_bb) != 0) || 
                    ((MoveGenerator.KingMoves(_sq) & bitboards[Piece.King] & color_bb) != 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong AttackersTo(byte _sq, byte _byColor)
        {
            return AttackersTo(_sq, _byColor, Occupancy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong AttackersTo (byte _sq , byte _byColor, ulong _occ)
        {
            ulong color_bb = bitboards[_byColor];
            return (MoveGenerator.PawnAttacksMoves(_sq, (byte)(_byColor ^ 1)) & bitboards[Piece.Pawn] & color_bb) | 
                   (MoveGenerator.KnightMoves(_sq) & bitboards[Piece.Knight] & color_bb ) | 
                   (MoveGenerator.BishopMoves(_occ, _sq) & (bitboards[Piece.Bishop] | bitboards[Piece.Queen]) & color_bb) | 
                   (MoveGenerator.RookMoves(_occ, _sq) & (bitboards[Piece.Rook] | bitboards[Piece.Queen]) & color_bb) | 
                   (MoveGenerator.KingMoves(_sq) & bitboards[Piece.King] & color_bb);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetAllAttackersTo(byte _sq)
        {
            return GetAllAttackersTo(_sq, Occupancy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetAllAttackersTo(byte _sq, ulong _occ)
        {
            return  (MoveGenerator.PawnAttacksMoves(_sq, Piece.Black) & bitboards[Piece.Pawn] & bitboards[Piece.White]) |
                (MoveGenerator.PawnAttacksMoves(_sq, Piece.White) & bitboards[Piece.Pawn] & bitboards[Piece.Black]) |
                (MoveGenerator.KnightMoves(_sq) & bitboards[Piece.Knight]) |
                (MoveGenerator.BishopMoves(_occ, _sq) & (bitboards[Piece.Bishop] | bitboards[Piece.Queen])) |
                (MoveGenerator.RookMoves(_occ, _sq) & (bitboards[Piece.Rook] | bitboards[Piece.Queen])) |
                (MoveGenerator.KingMoves(_sq) & bitboards[Piece.King]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetMayorAndMinorPieces(byte _color)
        {
            return (bitboards[Piece.Knight] | bitboards[Piece.Bishop] | bitboards[Piece.Rook] | bitboards[Piece.Queen]) & bitboards[_color]; 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetBitboard(byte _color, byte _pieceType)
        {
            return bitboards[_color] & bitboards[_pieceType];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte PieceOn(byte _sq)
        {
            for (byte pc = Piece.Pawn; pc <= Piece.King; pc++)
                if (BitUtils.Contains(bitboards[pc], _sq)) return pc;

            return Piece.None;
        }



        public void Dispose()
        {
            ClearPosition();
        }

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("ChessBoard:\n");
            ulong mask;
            byte sq, file,rank,pieceIndex;
            char sq_char = '.';
            for (rank = 0; rank < 8; rank++)
            {
                builder.Append($" {8 - rank}  ");
                for (file = 0; file < 8; file++)
                {
                    sq = (byte)((rank << 3) + file);
                    sq ^= 56;
                    mask  = (1UL << sq);
                    for (pieceIndex = Piece.Pawn; pieceIndex <= Piece.King; pieceIndex++)
                    {
                        if ((bitboards[pieceIndex] & mask) != 0)
                        {
                            if ((mask & bitboards[Piece.White]) != 0)
                            {
                                sq_char = char.ToUpper(Notation.PIECES_PRINT[pieceIndex]);
                            }else 
                            {
                                sq_char = Notation.PIECES_PRINT[pieceIndex];
                            }
                            break;
                        }else 
                        {
                            sq_char = '.';
                        }
                    }
                    builder.AppendFormat("{0} ", sq_char);
                }
                builder.AppendLine();
            }
            builder.AppendFormat("{0,5}{1,2}{2,2}{3,2}{4,2}{5,2}{6,2}{7,2}" , "A" , "B" , "C" , "D" , "E" , "F" , "G" , "H");
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendFormat("to move: {0}\n" , sideToMove == Piece.White ? "White" : "Black");
            builder.Append("castling: ");
            if (castleRights != CastleRights.None)
            {
                if ((castleRights & CastleRights.KingSide_White) != 0)
                {
                    builder.Append("K");
                }

                if ((castleRights & CastleRights.QueenSide_White) != 0)
                {
                    builder.Append("Q");
                }

                if ((castleRights & CastleRights.KingSide_Black) != 0)
                {
                    builder.Append("k");
                }

                if ((castleRights & CastleRights.QueenSide_Black) != 0)
                {
                    builder.Append("q");
                }

                builder.AppendLine();
            }else 
            {
                builder.Append("none\n");
            }

            if (ep_square != 0)
            {
                builder.Append($"en-passant: {(BoardSquare)ep_square}\n");
            }else 
            {
                builder.Append("en-passant: none\n");
            }
            builder.Append($"half moves:  {halfMoves}\n");
            builder.Append($"key: {key:X}\n");
            builder.Append($"key pawns: {key_pawns:X}\n");
            builder.Append($"fen: {Notation.GenerateFEN(this)}");

            return builder.ToString();
        }

        public override bool Equals(object obj)
		{
            if (obj is ChessBoard board)
            {
                return board.bitboards != null && board.bitboards[Piece.White] == bitboards[Piece.White] && 
                       board.bitboards[Piece.Black] == bitboards[Piece.Black] && 
                       board.bitboards[Piece.Pawn] == bitboards[Piece.Pawn] && 
                       board.bitboards[Piece.Knight] == bitboards[Piece.Knight] && 
                       board.bitboards[Piece.Bishop] == bitboards[Piece.Bishop] && 
                       board.bitboards[Piece.Rook] == bitboards[Piece.Rook] && 
                       board.bitboards[Piece.Queen] == bitboards[Piece.Queen] && 
                       board.bitboards[Piece.King] == bitboards[Piece.King] &&
                       board.sideToMove == sideToMove && board.ep_square == ep_square &&
                       board.castleRights == castleRights && board.halfMoves == halfMoves && 
                       board.key == key && board.key_pawns == key_pawns;
            }
            return false;
        }

		public override int GetHashCode()
		{
            return (int)(key >> 48 | key_pawns >> 16);
		}
	}
}