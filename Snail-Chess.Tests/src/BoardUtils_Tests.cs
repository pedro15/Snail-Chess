using NUnit.Framework;
using SnailChess.Core;

namespace SnailChess.Tests
{
    public class BoardUtils_Tests
    {   
        [Test]
        public void SquareCoordsUtilsTest()
        {
            BoardSquare sq = BoardSquare.a4;
            Assert.That(BoardUtils.SquareToRank((byte)sq), Is.EqualTo(4));
            Assert.That(BoardUtils.SquareToFile((byte)sq), Is.EqualTo(1));
        }
    }    
}