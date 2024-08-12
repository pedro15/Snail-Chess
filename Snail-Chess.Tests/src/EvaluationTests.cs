using NUnit.Framework;
using SnailChess.Core;
using SnailChess.AI.Evaluation;
using SnailChess.AI;
using System.Collections.Generic;

namespace  SnailChess.Tests
{
    public class EvaluationTests 
    {

        [Test]
        public void Test_PassedPawns()
        {
            System.Tuple<string, KeyValuePair<BoardSquare, bool>[]>[] passed_pawn_positions =
            {
                // 1
                new System.Tuple<string, KeyValuePair<BoardSquare, bool>[]>("k7/2p5/8/2P2PP1/4p3/8/8/7K w - - 0 1" , new KeyValuePair<BoardSquare, bool>[]
                {
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.g5, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.f5, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.c5, false),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.e4, true),
                }),
                // 2
                new System.Tuple<string, KeyValuePair<BoardSquare, bool>[]>("k7/2p5/8/2P2PP1/8/8/8/7K w - - 0 1" , new KeyValuePair<BoardSquare, bool>[] 
                {
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.g5, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.f5, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.c5, false)
                }),
                // 3
                new System.Tuple<string, KeyValuePair<BoardSquare, bool>[]>("k7/2p5/5q2/2P2PP1/8/8/8/7K w - - 0 1" , new KeyValuePair<BoardSquare, bool>[] 
                {
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.g5, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.f5, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.c5, false)
                }),
                // 4
                new System.Tuple<string, KeyValuePair<BoardSquare, bool>[]>("k7/2p2p2/5n2/2P2PP1/8/8/8/7K w - - 0 1" , new KeyValuePair<BoardSquare, bool>[] 
                {
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.g5, false),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.f5, false),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.c5, false)
                }),
                // 5
                new System.Tuple<string, KeyValuePair<BoardSquare, bool>[]>("k7/2p1p3/5n2/2P2PP1/8/8/8/7K w - - 0 1" , new KeyValuePair<BoardSquare, bool>[] 
                {
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.g5, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.f5, false),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.c5, false)
                }),
                // 6
                new System.Tuple<string, KeyValuePair<BoardSquare, bool>[]>("7k/8/7p/1P2Pp1P/2Pp1PP1/8/8/K7 w - - 0 1" , new KeyValuePair<BoardSquare, bool>[]
                {
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.b5, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.c4, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.e5, true),
                    new KeyValuePair<BoardSquare, bool>(BoardSquare.d4, true),
                })
            };

            ChessBoard board = new ChessBoard();
            ulong bb, bb_opponent;
            byte sq;
            bool result;
            System.Tuple<string, KeyValuePair<BoardSquare, bool>[]> test_data;

            for (int i = 0; i < passed_pawn_positions.Length; i++)
            {
                test_data = passed_pawn_positions[i];
                board.LoadPosition(Notation.ParseFEN(test_data.Item1));

                for (byte color = Piece.Black; color <= Piece.White; color++)
                {
                    bb = board.bitboards[color] & board.bitboards[Piece.Pawn];
                    bb_opponent = board.bitboards[Piece.FlipColor(color)] & board.bitboards[Piece.Pawn];

                    while(bb != 0)
                    {
                        sq = BitUtils.BitScan(bb);

                        result = PawnStructureUtil.IsPawnPassed(sq, color, in board.bitboards[Piece.Pawn], in bb_opponent);
                        bool found = false;
                        for (int j = 0; j < test_data.Item2.Length; j++)
                        {
                            if (test_data.Item2[j].Key == (BoardSquare)sq)
                            {
                                Assert.That(result, Is.EqualTo(test_data.Item2[j].Value), $"sq: {(BoardSquare)sq} | fen: {test_data.Item1} | test number: {i+1}");
                                found  = true;
                                break;
                            }
                        }

                        if (!found && result) 
                        {
                            Assert.Fail($"Unexpected passed pawn at: {(BoardSquare)sq} | fen: {test_data.Item1} | test number: {i+1}");
                        }

                        bb = BitUtils.PopLsb(bb);
                    }
                }
                
            }
        }
    }
}