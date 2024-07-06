using NUnit.Framework;
using SnailChess.Core;

namespace SnailChess.Tests
{
    public sealed class Move_Tests
    {
        [Test]
        public void TestEncoding()
        {
            byte from = (byte)BoardSquare.g7;
            byte to = (byte)BoardSquare.h8;
            MoveFlag flag = MoveFlag.Capture_Promotion_Queen;
			bool captured = true;

            Move move = new Move(from , to , flag);
            System.Console.WriteLine((ushort)move);

            Assert.That(move.from, Is.EqualTo(from));
            Assert.That(move.to, Is.EqualTo(to));
            Assert.That(move.flag, Is.EqualTo(flag));
			Assert.That(move.IsCapture, Is.EqualTo(captured));
        }
        
        [Test]
        public void TestParsing()
        {
            void ValidateMove(Move _actual, Move _expected)
            {
                Assert.That(_actual.from, Is.EqualTo(_expected.from));
                Assert.That(_actual.to, Is.EqualTo(_expected.to));
                Assert.That(_actual.flag, Is.EqualTo(_expected.flag));
            }

            Move expected_move, loaded_move;
            string move_str = "";
            Move[] legal_moves = null;
            BoardPosition pos;
            
            // ======== > Pawn double push
            move_str = "a2a4";
            expected_move = new Move(BoardSquare.a2, BoardSquare.a4, MoveFlag.PawnDoublePush);
            // ------------------------------------------------------------------------
            pos = Notation.ParseFEN(Notation.POSITION_DEFAULT);
            legal_moves = Move.GetLegalMoves(in pos);
            loaded_move = Move.FromLAN(move_str, in legal_moves);

            ValidateMove(loaded_move, expected_move);
            // ------------------------------------------------------------------------

            // ======== > Simple capture
            move_str = "e6d8";
            expected_move = new Move(BoardSquare.e6, BoardSquare.d8, MoveFlag.Capture);
            // ----------------------------------------------------------------------
            pos = Notation.ParseFEN("r1bqkb1r/pppppppp/2n1Nn2/8/3Q4/2N5/PPPPPPPP/R1BQKB1R w KQkq - 0 1");
            legal_moves = Move.GetLegalMoves(in pos);
            loaded_move = Move.FromLAN(move_str, in legal_moves);
            
            ValidateMove(loaded_move, expected_move);
            // ------------------------------------------------------------------------


            // ======== > Simple promotion
            move_str = "g7g8q";
            expected_move = new Move(BoardSquare.g7, BoardSquare.g8, MoveFlag.Promotion_Queen);
            // ----------------------------------------------------------------------
            pos = Notation.ParseFEN("1k6/6P1/8/8/8/8/5K2/8 w - - 0 1");
            legal_moves = Move.GetLegalMoves(in pos);
            loaded_move = Move.FromLAN(move_str, in legal_moves);
            
            ValidateMove(loaded_move, expected_move);
            // ------------------------------------------------------------------------

            // ======== > Promotion Capture
            move_str = "g7f8q";
            expected_move = new Move(BoardSquare.g7, BoardSquare.f8, MoveFlag.Capture_Promotion_Queen);
            // ----------------------------------------------------------------------
            pos = Notation.ParseFEN("1k3r2/6P1/8/8/8/8/5K2/8 w - - 0 1");
            legal_moves = Move.GetLegalMoves(in pos);
            loaded_move = Move.FromLAN(move_str, in legal_moves);
            
            ValidateMove(loaded_move, expected_move);
            // ------------------------------------------------------------------------

            // ======== > Castle -- kingside
            move_str = "e1g1";
            expected_move = new Move(BoardSquare.e1, BoardSquare.g1, MoveFlag.CastleKing);
            // ----------------------------------------------------------------------
            pos = Notation.ParseFEN("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1");
            legal_moves = Move.GetLegalMoves(in pos);
            loaded_move = Move.FromLAN(move_str, in legal_moves);
            
            ValidateMove(loaded_move, expected_move);
            // ------------------------------------------------------------------------

            // ======== > Castle -- QueenSide
            move_str = "e1c1";
            expected_move = new Move(BoardSquare.e1, BoardSquare.c1, MoveFlag.CastleQueen);
            // ----------------------------------------------------------------------
            pos = Notation.ParseFEN("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1");
            legal_moves = Move.GetLegalMoves(in pos);
            loaded_move = Move.FromLAN(move_str, in legal_moves);
            
            ValidateMove(loaded_move, expected_move);
            // ------------------------------------------------------------------------

        }
    }
}