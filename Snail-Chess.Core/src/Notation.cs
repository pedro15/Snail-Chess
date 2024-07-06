using System.Collections.Generic;
using SnailChess.Core.Hashing;
using System.Text;
using System;

namespace SnailChess.Core
{
    public static class Notation
    {
        public const string POSITION_DEFAULT = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public const string POSITION_KIWIPETE = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

        // (black = b)(white = w)(pawn = i)(knight = n)(bishop = b)(rook = r)(queen = q)(king = k)
        public const string PIECES_PRINT = "bwinbrqk*";
        public const string PIECES_FEN = "**pnbrqk*";
        public static readonly string[] NAMES = { "Black", "White", "Pawn", "Knight", "Bishop", "Rook", "Queen", "King", "None" };

        public static string PrintBitboard(ulong _bitboard)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            ulong mask = 0UL;
            byte sq, file,rank;
            for (rank = 0; rank < 8; rank++)
            {
                builder.Append($" {8 - rank}  ");
                for (file = 0; file < 8; file++)
                {
                    sq = (byte)(((rank << 3) + file) ^ 56);
                    mask  = 1UL << sq;
                    if ((_bitboard & mask) != 0)
                    {
                        builder.Append("1 ");
                    }else 
                    {
                        builder.Append(". ");
                    }
                }
                builder.AppendLine();
            }
            builder.AppendFormat("{0,5}{1,2}{2,2}{3,2}{4,2}{5,2}{6,2}{7,2}" , "A" , "B" , "C" , "D" , "E" , "F" , "G" , "H");
            builder.AppendLine();
            builder.AppendFormat(" 0x{0:X} \n\n" , _bitboard);
            return builder.ToString();
        }
        
        public static bool IsValidFEN(string _fen)
        {
            string[] fen_sections = _fen.Trim().Split(new char[] { ' ' } , System.StringSplitOptions.RemoveEmptyEntries);
            if (fen_sections.Length < 3) return false;
            int lookupIndex = 0 , i = -1;
            
            byte piece_count = 0;
            byte val;
            char current_letter;

            // Piece placement
            for (i = 0; i < fen_sections[lookupIndex].Length; i++)
            {
                current_letter = fen_sections[lookupIndex][i];
                switch(current_letter)
                {
                    case 'p':
                    case 'P':
                    case 'r':
                    case 'R':
                    case 'n':
                    case 'N':
                    case 'b':
                    case 'B':
                    case 'q':
                    case 'Q':
                    case 'K':
                    case 'k':
                    piece_count++;
                    if (piece_count > 8)
                    {
                        return false;
                    }
                    break;
                    case '/':
                    if (piece_count != 8)
                    {
                        return false;
                    }else 
                    {
                        piece_count = 0;
                    }
                    break;

                    default:
                    if (char.IsDigit(current_letter))
                    {
                        val = (byte)char.GetNumericValue(current_letter);
                        if (val > 0 && val <= 8)
                        {
                            piece_count += val;
                        }else 
                        {
                            return false;
                        }
                    }else 
                    {
                        return false;
                    }
                    break;
                }
            }

            lookupIndex++;
            if (fen_sections[lookupIndex] != "w" && fen_sections[lookupIndex] != "b") return false;
            lookupIndex++;

            string castle_lower = fen_sections[lookupIndex].ToLower();
            if (castle_lower != "-")
            {
                for (i = 0; i < castle_lower.Length; i++)
                {
                    if (castle_lower[i] != 'k' && castle_lower[i] != 'q') return false;
                }
            }

            return true;
        }

        public static BoardPosition ParseFEN(string _fen)
        {   
            BoardPosition positionData = BoardPosition.EmptyPosition();
            int lookupIndex = 0;
            string[] fen_sections = _fen.Trim().Split(new char[] { ' ' } , System.StringSplitOptions.RemoveEmptyEntries);
            if (fen_sections.Length < 3) return positionData;

            // Piece placement
            char pp_current_piece;
            byte pp_sq = 0, pp_tsq = 0;
            for (int i = 0; i < fen_sections[lookupIndex].Length; i++)
            {  
                pp_current_piece = fen_sections[lookupIndex][i];
                if (pp_current_piece == '/') continue;

                if (char.IsDigit(pp_current_piece))
                {
                    pp_sq += byte.Parse(fen_sections[lookupIndex][i].ToString());
                    continue;
                }

                pp_tsq = (byte)(pp_sq ^ 56);
                switch(fen_sections[lookupIndex][i])
                {
                    case 'P': 
                        positionData.bitboards[Piece.White] |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Pawn]  |= (1UL << pp_tsq);
                    break;

                    case 'p':
                        positionData.bitboards[Piece.Black] |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Pawn]  |= (1UL << pp_tsq);
                    break; 

                    case 'N':
                        positionData.bitboards[Piece.White]  |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Knight] |= (1UL << pp_tsq);
                    break;

                    case 'n':
                        positionData.bitboards[Piece.Black]  |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Knight] |= (1UL << pp_tsq);
                    break;

                    case 'B':
                        positionData.bitboards[Piece.White]  |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Bishop] |= (1UL << pp_tsq);
                    break;
                    
                    case 'b':
                        positionData.bitboards[Piece.Black]  |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Bishop] |= (1UL << pp_tsq);
                    break;

                    case 'r':
                        positionData.bitboards[Piece.Black] |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Rook]  |= (1UL << pp_tsq);
                    break;

                    case 'R':
                        positionData.bitboards[Piece.White] |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Rook]  |= (1UL << pp_tsq);
                    break;

                    case 'q':
                        positionData.bitboards[Piece.Black] |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Queen] |= (1UL << pp_tsq);
                    break;

                    case 'Q':
                        positionData.bitboards[Piece.White] |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.Queen] |= (1UL << pp_tsq);
                    break;

                    case 'k':
                        positionData.bitboards[Piece.Black] |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.King]  |= (1UL << pp_tsq);
                    break;

                    case 'K':
                        positionData.bitboards[Piece.White] |= (1UL << pp_tsq);
                        positionData.bitboards[Piece.King]  |= (1UL << pp_tsq);
                    break;
                }
                pp_sq++;
            }
            lookupIndex++;

            // Side to move
            positionData.sideToMove = (fen_sections[lookupIndex] == "w") ? Piece.White : Piece.Black;
            lookupIndex++;
            
            // Castling
            for (int i = 0; i < fen_sections[lookupIndex].Length; i++)
            {
                switch(fen_sections[lookupIndex][i])
                {
                    case 'K' : positionData.castleRights |= CastleRights.KingSide_White; break;
                    case 'k' : positionData.castleRights |= CastleRights.KingSide_Black; break;
                    case 'Q' : positionData.castleRights |= CastleRights.QueenSide_White; break;
                    case 'q' : positionData.castleRights |= CastleRights.QueenSide_Black; break;
                }
            }
            lookupIndex++;

            if (lookupIndex < fen_sections.Length)
            {
                // en-passant
                if (fen_sections[lookupIndex] != "-" && System.Enum.TryParse<BoardSquare>(fen_sections[lookupIndex], false, out BoardSquare ep_sq))
                {
                    positionData.ep_square = (byte)ep_sq;
                }else 
                {
                    positionData.ep_square = 0;
                }
                lookupIndex++;
            }

            if (lookupIndex < fen_sections.Length)
            {
                // half moves
                if (byte.TryParse(fen_sections[lookupIndex], out byte half_moves))
                {
                    positionData.halfmoves = half_moves;
                }
                lookupIndex++;
            }

            if (lookupIndex < fen_sections.Length)
            {
                // full moves
                if (byte.TryParse(fen_sections[lookupIndex], out byte full_moves))
                {
                    positionData.fullmoves = full_moves;
                }
            }

            positionData.key = BoardHash.ComputeHash(in positionData);
            positionData.key_pawns = PawnHash.ComputeHash(in positionData);

            return positionData;
        }
        
        public static string GenerateFEN(in ChessBoard _board)
        {
            StringBuilder fen = new StringBuilder();
            if (_board.bitboards.Length != 8) return string.Empty;

            byte steps = 0, square, piece, color;
            sbyte rank,file;

            for (rank = 7; rank >= 0; rank--)
            {
                for (file = 0; file < 8; file++)
                {
                    square = BoardUtils.GetSquare((byte)file, (byte)rank);
                    if (BitUtils.Contains(_board.Occupancy, square))
                    {
                        if (steps > 0)
                        {
                            fen.Append(steps);
                            steps = 0;
                        }

                        for (piece = Piece.Pawn; piece <= Piece.King; piece++)
                        {
                            for (color = Piece.Black; color <= Piece.White; color++)
                            {
                                if (BitUtils.Contains(_board.bitboards[piece] & _board.bitboards[color], square))
                                {
                                    if (color == Piece.White)
                                    {
                                        //pc_char = char.ToUpper(PIECES_FEN[piece]);
                                        fen.Append(char.ToUpper(PIECES_FEN[piece]));
                                    }else
                                    {
                                        //pc_char = PIECES_FEN[piece];
                                        fen.Append(char.ToLower(PIECES_FEN[piece]));
                                    }
                                    break;
                                }
                            }
                        }
                    }else 
                    {
                        steps++;
                        if (steps >= 8)
                        {
                            fen.Append(steps);
                            steps = 0;
                        }
                    }

                    if (file == 7 && rank > 0)
                    {
                        if (steps > 0)
                        {
                            fen.Append(steps);
                            steps = 0;
                        }
                        fen.Append("/");
                    }
                }
            }

            // Side to move
            fen.Append((_board.sideToMove == Piece.White) ? " w " : " b ");
            // Castling rights
            if (_board.castleRights != CastleRights.None )
            {
                if ((_board.castleRights & CastleRights.KingSide_White)  != CastleRights.None) fen.Append("K");
                if ((_board.castleRights & CastleRights.QueenSide_White) != CastleRights.None) fen.Append("Q");
                if ((_board.castleRights & CastleRights.KingSide_Black)  != CastleRights.None) fen.Append("k");
                if ((_board.castleRights & CastleRights.QueenSide_Black) != CastleRights.None) fen.Append("q");
				fen.Append(" ");
            }
            else
            {
                fen.Append("- ");
            }

            // En passant target square
            if (_board.ep_square != 0)
            {
                fen.AppendFormat("{0} ", $"{(BoardSquare)_board.ep_square}");
            }
            else
            {
                fen.Append("- ");
            }
            
            // Half move clock
            fen.Append(_board.halfMoves);
            // full move counter
            fen.Append($" {_board.fullMoves}");

			return fen.ToString();
        }

    }
}