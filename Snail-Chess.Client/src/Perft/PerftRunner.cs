using SnailChess.Core;
using System;
using System.Diagnostics;

namespace SnailChess.Client.Perft
{
    public sealed class PerftRunner
    {
        private ChessBoard board = null;

        public float elapsedTime 
        {
            get 
            {
                if (stopwatch != null)
                {
                    return stopwatch.ElapsedMilliseconds / 1000f;
                }
                return 0f;
            }
        }
        
        public ulong totalNodes  {get; private set;}
        private Stopwatch stopwatch = null;

        public PerftRunner(BoardPosition _position)
        {
            board = new ChessBoard(_position);
        }

        public void RunPerft(int _depth, Func<string, ulong, bool> _OnPerftUpdate = null)
        {
            stopwatch = Stopwatch.StartNew();
            
            totalNodes = 0UL;
            ulong current_count = 0UL;
            Span<Move> moves = stackalloc Move[Move.MAX_MOVES];
            short target_index = board.GenerateMoves(in moves);
            

            for (short i = 0; i <= target_index; i++)
            {
                if (board.MakeMove(in moves[i]))
                {
                    current_count = Run(_depth - 1);
                    totalNodes += current_count;
                    board.UndoLastMove();
                    
                    if (_OnPerftUpdate != null && !_OnPerftUpdate.Invoke(moves[i].ToString() , current_count))
                    {
                        break;
                    }                    
                }
            }

            stopwatch.Stop();
        }

        private ulong Run(int _depth)
        {
            if (_depth == 0) return 1UL;

            ulong node_count = 0;
            Span<Move> moves = stackalloc Move[Move.MAX_MOVES];
            short target_index =  board.GenerateMoves(in moves);

            for (short i = 0; i <= target_index; i++)
            {
                if (board.MakeMove(in moves[i]))
                {
                    node_count += Run(_depth -1);
                    board.UndoLastMove();
                }
            }
            
            return node_count;
        }
    }
}