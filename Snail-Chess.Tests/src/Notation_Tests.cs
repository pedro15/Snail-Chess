using NUnit.Framework;
using SnailChess.Core;

namespace SnailChess.Tests
{
    internal sealed class Notation_Tests
    {
        [Test]
        public void FENValidate()
        {
            Assert.That(Notation.IsValidFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") , Is.EqualTo(true));
            Assert.That(Notation.IsValidFEN("rnbqkbnr/ppppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") , Is.EqualTo(false));
            Assert.That(Notation.IsValidFEN("rnbqkbnr/pppppppp/1/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") , Is.EqualTo(false));
            Assert.That(Notation.IsValidFEN("rnbqkbnr/pppppppp/1/8/8/8/PPPPPPPP/RNXBQKBNR w KQkq - 0 1") , Is.EqualTo(false));
            Assert.That(Notation.IsValidFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1") , Is.EqualTo(false));
            Assert.That(Notation.IsValidFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkx - 0 1") , Is.EqualTo(false));
        }
        
        [Test]
        public void FENGenerate()
        {
            string[] test_positions = 
            {
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
                "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
                "rnbqkbnr/pppppp1p/6p1/8/1P6/5N2/P1PPPPPP/RNBQKB1R w KQkq b3 0 1",
                "4k2r/6K1/8/8/8/8/8/8 w k - 0 3",
                "8/2k1p3/3pP3/3P2K1/8/8/8/8 w - - 0 1",
                "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N w - - 0 1",
                "rnbqkb1r/ppppp1pp/7n/4Pp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3"
            };

            ChessBoard dummy = new ChessBoard();
            string current_fen;
            BoardPosition current_pos;
            for (int i = 0; i < test_positions.Length; i++)
            {
                current_fen = test_positions[i];
                current_pos = Notation.ParseFEN(current_fen);
                dummy.LoadPosition(in current_pos);
                Assert.That(Notation.GenerateFEN(in dummy), Is.EqualTo(current_fen));
            }
        }
    }
}