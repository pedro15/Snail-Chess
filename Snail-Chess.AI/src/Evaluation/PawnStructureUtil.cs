using System.Runtime.CompilerServices;
using SnailChess.Core.MoveGen;
using SnailChess.Core;

namespace SnailChess.AI.Evaluation
{
    public static class PawnStructureUtil 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPawnPassed(byte _sq, byte _pawnColor, in ulong _pawns_bb, in ulong _opponent_bb)
        {
            return (EvaluationUtils.PassedPawns_masks[_pawnColor][_sq] & _pawns_bb & _opponent_bb) == 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPawnIsolated(byte _sq, in ulong _own_pawns_bb)
        {
            return (_own_pawns_bb & EvaluationUtils.IsolatedPawns_masks[_sq]) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPawnProtected(byte _sq, byte _pawnColor, in ulong _own_pawns_bb)
        {
            return (MoveGenerator.PawnAttacksMoves(_sq, Piece.FlipColor(_pawnColor)) & _own_pawns_bb) != 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPawnDoubled(byte _sq, in ulong _own_pawns_bb)
        {
            return BitUtils.Count(BoardUtils.FILES[BoardUtils.SquareToFile(_sq)] & _own_pawns_bb) > 1;
        }

    }
}