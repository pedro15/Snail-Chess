using NUnit.Framework;
using SnailChess.Core;
using SnailChess.Core.Hashing;

namespace SnailChess.Tests
{
    public sealed class ChessBoard_Tests 
    {
        [Test]
        public void SquareAttackedTest()
        {
            ChessBoard board = new ChessBoard(Notation.ParseFEN("rnbq1bnr/ppp2ppp/3p4/3k4/2P5/8/PP1P1PPP/RNBQKBNR w KQha - 0 1"));
            Assert.That(board.IsAttacked((byte)BoardSquare.c4, Piece.Black), Is.EqualTo(true));

            board.LoadPosition(Notation.ParseFEN("8/4n3/8/3P4/8/8/8/8 w - - 0 1"));
            Assert.That(board.IsAttacked((byte)BoardSquare.d5, Piece.Black), Is.EqualTo(true));

            board.LoadPosition(Notation.ParseFEN("8/8/4p3/3P4/8/8/8/8 w - - 0 1"));
            Assert.That(board.IsAttacked((byte)BoardSquare.d5, Piece.Black), Is.EqualTo(true));

            board.LoadPosition(Notation.ParseFEN("8/8/4b3/3P4/8/8/8/8 w - - 0 1"));
            Assert.That(board.IsAttacked((byte)BoardSquare.d5, Piece.Black), Is.EqualTo(true));

            board.LoadPosition(Notation.ParseFEN("8/8/4r3/3P4/8/8/8/8 w - - 0 1"));
            Assert.That(board.IsAttacked((byte)BoardSquare.d5, Piece.Black), Is.EqualTo(false));

            board.LoadPosition(Notation.ParseFEN("8/8/4q3/3P4/8/8/8/8 w - - 0 1"));
            Assert.That(board.IsAttacked((byte)BoardSquare.d5, Piece.Black), Is.EqualTo(true));

            board.LoadPosition(Notation.ParseFEN("8/8/4k3/3P4/8/8/8/8 w - - 0 1"));
            Assert.That(board.IsAttacked((byte)BoardSquare.d5, Piece.Black), Is.EqualTo(true));
        }
        
        [Test]
        public void BoardHashTest()
        {
            ChessBoard board = new ChessBoard();
            
            void PerformTest(string _fen)
            {
                board.LoadPosition(Notation.ParseFEN(_fen));
                System.Span<Move> moves = stackalloc Move[Move.MAX_MOVES];
                short move_index = board.GenerateMoves(in moves);
                Move current_move;
                for (short i = 0; i <= move_index; i++)
                {
                    current_move = moves[i];

                    if (board.MakeMove(in current_move))
                    {
                        Assert.That(board.key, Is.EqualTo(BoardHash.ComputeHash(board.ExportPosition())), () => $"move_played: {current_move}\n{board}");
                        board.UndoLastMove();
                    }
                }
            }
            
            PerformTest(Notation.POSITION_KIWIPETE);
            PerformTest("1k4n1/5P2/8/8/8/8/p7/3K4 w - - 0 1");
            PerformTest("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1");
            PerformTest("r3k1r1/8/8/8/8/8/8/R3K2R b KQq");
            PerformTest("8/8/3K4/3Nn3/3nN3/4k3/8/8 b");
            PerformTest("K7/b7/1b6/1b6/8/8/8/k6B b");
            PerformTest("8/8/7k/7p/7P/7K/8/8 w ");
            PerformTest("3k4/3pp3/8/8/8/8/3PP3/3K4 b");
            PerformTest("n1n5/1Pk5/8/8/8/8/5Kp1/5N1N w");
        }

        [Test]
        public void PawnHashTest()
        {
            ChessBoard board = new ChessBoard();
            void PerformTest(string _fen)
            {
                board.LoadPosition(Notation.ParseFEN(_fen));
                System.Span<Move> moves = stackalloc Move[Move.MAX_MOVES];
                short move_index = board.GenerateMoves(in moves);
                Move current_move;
                for (short i = 0; i <= move_index; i++)
                {
                    current_move = moves[i];

                    if (board.MakeMove(in current_move))
                    {
                        Assert.That(board.key_pawns, Is.EqualTo(PawnHash.ComputeHash(board.ExportPosition())), () => $"move_played: {current_move}\n{board}");
                        board.UndoLastMove();
                    }
                }
            }

            PerformTest(Notation.POSITION_KIWIPETE);
            PerformTest("1k4n1/5P2/8/8/8/8/p7/3K4 w - - 0 1");
            PerformTest("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1");
            PerformTest("r3k1r1/8/8/8/8/8/8/R3K2R b KQq");
            PerformTest("8/8/3K4/3Nn3/3nN3/4k3/8/8 b");
            PerformTest("K7/b7/1b6/1b6/8/8/8/k6B b");
            PerformTest("8/8/7k/7p/7P/7K/8/8 w ");
            PerformTest("3k4/3pp3/8/8/8/8/3PP3/3K4 b");
            PerformTest("n1n5/1Pk5/8/8/8/8/5Kp1/5N1N w");
            PerformTest("rnbqkbnr/pp1p1ppp/8/1Pp1p3/3P4/8/P1P1PPPP/RNBQKBNR w KQkq c6 0 1");
        }
    }
}