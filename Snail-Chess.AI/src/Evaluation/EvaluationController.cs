using System;
using System.Runtime.CompilerServices;
using SnailChess.AI.HashTables;
using SnailChess.Core;
using SnailChess.Core.MoveGen;
using static SnailChess.AI.Evaluation.EvaluationUtils;

namespace SnailChess.AI.Evaluation
{
    public sealed class EvaluationController
    { 
        // [PieceType]
        private readonly int[] MATERIAL_TABLE_MG;
        private readonly int[] MATERIAL_TABLE_EG;

        // piece-square-tables
        // [PieceType][square]
        private readonly int[][] PST_TABLE_MG;
        private readonly int[][] PST_TABLE_EG;

        // Pawn evaluation Params
        private readonly int[] PASSED_PAWN_BONUS;
        private readonly int[] PASSED_PAWN_BONUS_EG;


        private short ISOLATED_PAWN_PENALTY;  
        private short DOUBLE_PAWN_PENALTY;
        private short DOUBLE_PAWN_PENALTY_EG;
        private short PROTECTED_PASSED_PAWN_BONUS;
        private short PROTECTED_PASSED_PAWN_BONUS_EG;
        // Tempo bonus
        private short TEMPO;
        // King safety
        private int[] KING_SAFETY_ATTACK_WEIGHTS;
        // mobility
        private short BISHOP_MOBILITY_SCORE;
        private short BISHOP_MOBILITY_SCORE_EG;

        private short ROOK_MOBILITY_SCORE;
        private short ROOK_MOBILITY_SCORE_EG;

        private short QUEEN_MOBILITY_SCORE;
        private short QUEEN_MOBILITY_SCORE_EG;

        private int[] KNIGHT_PAWN_ADJ;
        private int[] ROOK_PAWN_ADJ;

        private short BISHOP_PAIR_SCORE;
        private short BISHOP_PAIR_SCORE_EG;

        private short KNIGHT_PAIR_SCORE;
        private short KNIGHT_PAIR_SCORE_EG;

        private short ROOK_PAIR_SCORE;
        private short ROOK_PAIR_SCORE_EG;

        private readonly HashTablePawns TT_pawns;
        private readonly bool use_cache;

        public EvaluationController(bool _use_cache = true)
        {
            use_cache = _use_cache;
            TT_pawns = new HashTablePawns();

            MATERIAL_TABLE_MG = new int[8];
            MATERIAL_TABLE_EG = new int[8];

            PST_TABLE_MG = new int[8][];
            PST_TABLE_EG = new int[8][];

            for (byte pc = Piece.Pawn; pc <= Piece.King; pc++)
            {
                PST_TABLE_MG[pc] = new int[64];
                PST_TABLE_EG[pc] = new int[64];
            }

            PASSED_PAWN_BONUS = new int[8];
            PASSED_PAWN_BONUS_EG = new int[8];
            KING_SAFETY_ATTACK_WEIGHTS = new int[8];

            KNIGHT_PAWN_ADJ = new int[9];
            ROOK_PAWN_ADJ = new int[9];
        }
        
        public void LoadParams(in EvaluationParams _searchParams)
        { 
            for (byte pc = Piece.Pawn; pc <= Piece.King; pc++)
            {
                MATERIAL_TABLE_MG[pc] = _searchParams.piece_material[pc];
                MATERIAL_TABLE_EG[pc] = _searchParams.piece_material_eg[pc];
                
                for (byte sq = 0; sq < 64; sq++)
                {
                    switch(pc)
                    {
                        case Piece.Pawn:
                        PST_TABLE_MG[Piece.Pawn][sq] = _searchParams.pst_pawn[sq];
                        PST_TABLE_EG[Piece.Pawn][sq] = _searchParams.pst_pawn_eg[sq];
                        break;
                        case Piece.Knight:
                        PST_TABLE_MG[Piece.Knight][sq] = _searchParams.pst_knight[sq];
                        PST_TABLE_EG[Piece.Knight][sq] = _searchParams.pst_knight_eg[sq];
                        break;
                        case Piece.Bishop:
                        PST_TABLE_MG[Piece.Bishop][sq] = _searchParams.pst_bishop[sq];
                        PST_TABLE_EG[Piece.Bishop][sq] = _searchParams.pst_bishop_eg[sq];
                        break;  
                        case Piece.Rook:
                        PST_TABLE_MG[Piece.Rook][sq] = _searchParams.pst_rook[sq];
                        PST_TABLE_EG[Piece.Rook][sq] = _searchParams.pst_rook_eg[sq];
                        break;
                        case Piece.Queen:
                        PST_TABLE_MG[Piece.Queen][sq] = _searchParams.pst_queen[sq];
                        PST_TABLE_EG[Piece.Queen][sq] = _searchParams.pst_queen_eg[sq];
                        break;
                        case Piece.King:
                        PST_TABLE_MG[Piece.King][sq] = _searchParams.pst_king[sq];
                        PST_TABLE_EG[Piece.King][sq] = _searchParams.pst_king_eg[sq];
                        break;
                    }
                }
            }

            Array.Copy(_searchParams.passed_pawn_bonus, PASSED_PAWN_BONUS, PASSED_PAWN_BONUS.Length);
            Array.Copy(_searchParams.passed_pawn_bonus_eg, PASSED_PAWN_BONUS_EG, PASSED_PAWN_BONUS_EG.Length);
            Array.Copy(_searchParams.king_safety_attack_weights, KING_SAFETY_ATTACK_WEIGHTS, KING_SAFETY_ATTACK_WEIGHTS.Length);
            Array.Copy(_searchParams.rook_pawn_adj, ROOK_PAWN_ADJ, ROOK_PAWN_ADJ.Length);
            Array.Copy(_searchParams.knight_pawn_adj, KNIGHT_PAWN_ADJ, KNIGHT_PAWN_ADJ.Length);

            PROTECTED_PASSED_PAWN_BONUS = _searchParams.protected_passed_pawn_bonus;
            PROTECTED_PASSED_PAWN_BONUS_EG = _searchParams.protected_passed_pawn_bonus_eg;

            DOUBLE_PAWN_PENALTY = _searchParams.double_pawn_penalty;
            DOUBLE_PAWN_PENALTY_EG = _searchParams.double_pawn_penalty_eg;
            ISOLATED_PAWN_PENALTY = _searchParams.isolated_pawn_penalty;
            TEMPO = _searchParams.tempo;

            BISHOP_MOBILITY_SCORE = _searchParams.bishop_mobility_score;
            BISHOP_MOBILITY_SCORE_EG = _searchParams.bishop_mobility_score_eg;

            ROOK_MOBILITY_SCORE = _searchParams.rook_mobility_score;
            ROOK_MOBILITY_SCORE_EG = _searchParams.rook_mobility_score_eg;

            QUEEN_MOBILITY_SCORE = _searchParams.queen_mobility_score;
            QUEEN_MOBILITY_SCORE_EG = _searchParams.queen_mobility_score_eg;

            BISHOP_PAIR_SCORE = _searchParams.bishop_pair_score;
            BISHOP_PAIR_SCORE_EG = _searchParams.bishop_pair_score_eg;

            ROOK_PAIR_SCORE = _searchParams.rook_pair_score;
            ROOK_PAIR_SCORE_EG = _searchParams.rook_pair_score_eg;

            KNIGHT_PAIR_SCORE = _searchParams.knight_pair_score;
            KNIGHT_PAIR_SCORE_EG = _searchParams.knight_pair_score_eg;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ComputePawnStructure(in ChessBoard _board, ref TaperedEvalScore _taperedScore)
        {
            ulong bb_piece, bb_own, pawns_bb = _board.bitboards[Piece.Pawn];
            int score_mg = 0, score_eg = 0;
            byte sq;
            bool is_white;

            for (byte color = Piece.Black; color <= Piece.White; color++)
            {
                is_white = color == Piece.White;
                bb_piece = _board.bitboards[color] & pawns_bb;
                bb_own = bb_piece;
                while (bb_piece != 0)
                {
                    sq = BitUtils.BitScan(bb_piece);

                    // PST + Material
                    if (is_white)
                    {
                        score_mg += PST_TABLE_MG[Piece.Pawn][BoardUtils.SquareInvert(sq)] + MATERIAL_TABLE_MG[Piece.Pawn];
                        score_eg += PST_TABLE_EG[Piece.Pawn][BoardUtils.SquareInvert(sq)] + MATERIAL_TABLE_EG[Piece.Pawn];
                    }else 
                    {
                        score_mg -= PST_TABLE_MG[Piece.Pawn][sq] + MATERIAL_TABLE_MG[Piece.Pawn];
                        score_eg -= PST_TABLE_EG[Piece.Pawn][sq] + MATERIAL_TABLE_EG[Piece.Pawn];
                    }
                    
                    // Doubled Pawn
                    if (PawnStructureUtil.IsPawnDoubled(sq, in bb_own))
                    {
                        if (is_white)
                        {
                            score_mg += DOUBLE_PAWN_PENALTY;
                            score_eg += DOUBLE_PAWN_PENALTY_EG;
                        }else 
                        {
                            score_mg -= DOUBLE_PAWN_PENALTY;
                            score_eg -= DOUBLE_PAWN_PENALTY_EG;
                        }
                    }

                    // Isolated Pawn
                    if (PawnStructureUtil.IsPawnIsolated(sq, in bb_own))
                    {
                        if (is_white)
                        {
                            score_mg += ISOLATED_PAWN_PENALTY;
                            score_eg += ISOLATED_PAWN_PENALTY;
                        }
                        else
                        {
                            score_mg -= ISOLATED_PAWN_PENALTY;
                            score_eg -= ISOLATED_PAWN_PENALTY;
                        }
                    }

                    if (PawnStructureUtil.IsPawnPassed(sq, color, in pawns_bb, in _board.bitboards[Piece.FlipColor(color)]))
                    {
                        // Passed pawn, apply bonus
                        if (is_white)
                        {
                            score_mg += PASSED_PAWN_BONUS[BoardUtils.SquareToRank(sq)];
                            score_eg += PASSED_PAWN_BONUS_EG[BoardUtils.SquareToRank(sq)];
                        }
                        else
                        {
                            score_mg -= PASSED_PAWN_BONUS[BoardUtils.SquareToRank(BoardUtils.SquareInvert(sq))];
                            score_eg -= PASSED_PAWN_BONUS_EG[BoardUtils.SquareToRank(BoardUtils.SquareInvert(sq))];
                        }

                        // protected passer
                        if (PawnStructureUtil.IsPawnProtected(sq, color, in bb_own))
                        {
                            if (is_white)
                            {
                                score_mg += PROTECTED_PASSED_PAWN_BONUS;
                                score_eg += PROTECTED_PASSED_PAWN_BONUS_EG;
                            }
                            else
                            {
                                score_mg -= PROTECTED_PASSED_PAWN_BONUS;
                                score_eg -= PROTECTED_PASSED_PAWN_BONUS_EG;
                            }
                        }
                    }

                    bb_piece = BitUtils.PopLsb(bb_piece);
                }
            }

            _taperedScore.score_mg = (short)score_mg;
            _taperedScore.score_eg = (short)score_eg;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TaperedEvalScore GetPawnStructureScore(in ChessBoard _board)
        {
            HashEntryPawns pawns_entry = TT_pawns.GetEntry(_board.key_pawns);
            if (use_cache && pawns_entry)
            {
                return pawns_entry.evalScore;
            }else 
            {
                ComputePawnStructure(in _board, ref pawns_entry.evalScore);
                pawns_entry.key = _board.key_pawns;
                TT_pawns.RecordEntry(pawns_entry);
                return pawns_entry.evalScore;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Evaluate(in ChessBoard _board)
        {
            // init game phase
            int gamePhase = 0;
            int score_mg = 0, score_eg = 0;
            ulong Occupancy  = _board.Occupancy, bb_piece, piece_attacks, opponent_king_zone, opponent_ray_mask;
            byte sq, opponent_king_sq,count_set, pawn_count, bishop_count, rook_count, knight_count;
            bool is_white;
            int king_danger;

            for (byte color = Piece.Black; color <= Piece.White; color++)
            {
                is_white = color == Piece.White;

                // init eval data for this color
                pawn_count = 0;
                knight_count = 0;
                bishop_count = 0;
                rook_count = 0;

                opponent_ray_mask = ~GetAttackMap(in _board, Piece.FlipColor(color));
                opponent_king_sq = BitUtils.BitScan(_board.bitboards[Piece.King] & _board.bitboards[Piece.FlipColor(color)]);
                opponent_king_zone = King_zone[opponent_king_sq];
                king_danger = 0;

                for (byte piece = Piece.Pawn; piece <= Piece.King; piece++)
                {
                    bb_piece = _board.bitboards[color] & _board.bitboards[piece];

                    if (piece == Piece.Pawn)
                    {
                        // compute only gamePhase for pawns, since pawns are evaluated separately
                        gamePhase += GAME_PHASE_VALUES[Piece.Pawn] * BitUtils.Count(bb_piece);
                        pawn_count = BitUtils.Count(_board.bitboards[Piece.Pawn] & _board.bitboards[color]);
                        continue;
                    }
                    
                    while(bb_piece != 0)
                    {
                        // current square
                        sq = BitUtils.BitScan(bb_piece);
                        
                        // update game phase
                        gamePhase += GAME_PHASE_VALUES[piece];

                        if (is_white)
                        {
                            score_mg += PST_TABLE_MG[piece][BoardUtils.SquareInvert(sq)] + MATERIAL_TABLE_MG[piece];
                            score_eg += PST_TABLE_EG[piece][BoardUtils.SquareInvert(sq)] + MATERIAL_TABLE_EG[piece];
                        }else 
                        {
                            score_mg -= PST_TABLE_MG[piece][sq] + MATERIAL_TABLE_MG[piece];
                            score_eg -= PST_TABLE_EG[piece][sq] + MATERIAL_TABLE_EG[piece];
                        }
                        
                        switch(piece)
                        {
                            
                            case Piece.Knight:

                                // update knight counter
                                knight_count++;

                                // Knight opponent king zone attacks
                                piece_attacks = MoveGenerator.KnightMoves(sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    king_danger += BitUtils.Count(opponent_king_zone & piece_attacks) * KING_SAFETY_ATTACK_WEIGHTS[Piece.Knight];
                                }

                                // adjust piece material based on number of pawns
                                if (is_white)
                                {
                                    score_mg += KNIGHT_PAWN_ADJ[pawn_count];
                                    score_eg += KNIGHT_PAWN_ADJ[pawn_count];
                                }else
                                {
                                    score_mg -= KNIGHT_PAWN_ADJ[pawn_count];
                                    score_eg -= KNIGHT_PAWN_ADJ[pawn_count];
                                }

                            break;

                            case Piece.Bishop:
                                
                                // update bishop counter
                                bishop_count++;

                                // Bishop opponent king zone attacks
                                piece_attacks = MoveGenerator.BishopMoves(in Occupancy, sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    king_danger += BitUtils.Count(opponent_king_zone & piece_attacks) * KING_SAFETY_ATTACK_WEIGHTS[Piece.Bishop];
                                }

                                // Bishop Mobility
                                if (piece_attacks != 0)
                                {
                                    count_set = BitUtils.Count(piece_attacks);
                                    if (is_white)
                                    {
                                        score_mg += count_set * BISHOP_MOBILITY_SCORE;
                                        score_eg += count_set * BISHOP_MOBILITY_SCORE_EG;
                                    }else 
                                    {
                                        score_mg -= count_set * BISHOP_MOBILITY_SCORE;
                                        score_eg -= count_set * BISHOP_MOBILITY_SCORE_EG;
                                    }
                                }

                            break;

                            case Piece.Rook:

                                // update rook counter
                                rook_count++;

                                // Rook opponent king zone attacks
                                piece_attacks = MoveGenerator.RookMoves(in Occupancy, sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    king_danger += BitUtils.Count(opponent_king_zone & piece_attacks) * KING_SAFETY_ATTACK_WEIGHTS[Piece.Rook];
                                }

                                // Rook Mobility
                                if (piece_attacks != 0)
                                {
                                    count_set = BitUtils.Count(piece_attacks);
                                    if (is_white)
                                    {
                                        score_mg += count_set * ROOK_MOBILITY_SCORE;
                                        score_eg += count_set * ROOK_MOBILITY_SCORE_EG;
                                    }else 
                                    {
                                        score_mg -= count_set * ROOK_MOBILITY_SCORE;
                                        score_eg -= count_set * ROOK_MOBILITY_SCORE_EG;
                                    }
                                }

                                // adjust piece materal based on number of pawns
                                if (is_white)
                                {
                                    score_mg += ROOK_PAWN_ADJ[pawn_count];
                                    score_eg += ROOK_PAWN_ADJ[pawn_count];
                                }else
                                {
                                    score_mg -= ROOK_PAWN_ADJ[pawn_count];
                                    score_eg -= ROOK_PAWN_ADJ[pawn_count];
                                }
                            break;

                            case Piece.Queen:
                                // Queen opponent king zone attacks
                                piece_attacks = MoveGenerator.QueenMoves(in Occupancy, sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    king_danger += BitUtils.Count(opponent_king_zone & piece_attacks) * KING_SAFETY_ATTACK_WEIGHTS[Piece.Queen];
                                }

                                // Queen Mobility
                                if (piece_attacks != 0)
                                {
                                    count_set = BitUtils.Count(piece_attacks);
                                    if (is_white)
                                    {
                                        score_mg += count_set * QUEEN_MOBILITY_SCORE;
                                        score_eg += count_set * QUEEN_MOBILITY_SCORE_EG;
                                    }else 
                                    {
                                        score_mg -= count_set * QUEEN_MOBILITY_SCORE;
                                        score_eg -= count_set * QUEEN_MOBILITY_SCORE_EG;
                                    }
                                }
                            break;

                            case Piece.King:
                                // King opponent king zone attacks
                                piece_attacks = MoveGenerator.KingMoves(sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    king_danger += BitUtils.Count(opponent_king_zone & piece_attacks) * KING_SAFETY_ATTACK_WEIGHTS[Piece.King];
                                }
                            break;
                        }

                        bb_piece = BitUtils.PopLsb(bb_piece);
                    }
                }

                if (is_white)
                {
                    // score king safety
                    score_mg += king_danger;
                    score_eg += king_danger;
                    
                    // score combination of different pieces
                    if (bishop_count > 1)
                    {
                        score_mg += BISHOP_PAIR_SCORE;
                        score_eg += BISHOP_PAIR_SCORE_EG;   
                    }

                    if (rook_count > 1)
                    {
                        score_mg += ROOK_PAIR_SCORE;
                        score_eg += ROOK_PAIR_SCORE_EG;
                    }

                    if (knight_count > 1)
                    {
                        score_mg += KNIGHT_PAIR_SCORE;
                        score_eg += KNIGHT_PAIR_SCORE_EG;
                    }
                }else 
                {
                    // score king safety
                    score_mg -= king_danger;
                    score_eg -= king_danger;
                    
                    // score combination of different pieces
                    if (bishop_count > 1)
                    {
                        score_mg -= BISHOP_PAIR_SCORE;
                        score_eg -= BISHOP_PAIR_SCORE_EG;   
                    }

                    if (rook_count > 1)
                    {
                        score_mg -= ROOK_PAIR_SCORE;
                        score_eg -= ROOK_PAIR_SCORE_EG;
                    }

                    if (knight_count > 1)
                    {
                        score_mg -= KNIGHT_PAIR_SCORE;
                        score_eg -= KNIGHT_PAIR_SCORE_EG;
                    }
                }
            }

            // Pawn evaluation
            TaperedEvalScore pawn_score = GetPawnStructureScore(in _board);
            score_mg += pawn_score.score_mg;
            score_eg += pawn_score.score_eg;

            // Tempo bonus
            if(_board.sideToMove == Piece.White)
            {
                score_mg += TEMPO;
            }else 
            {
                score_mg -= TEMPO;
            }

            int score = InterpolateScores(score_mg, score_eg, gamePhase);
            return _board.sideToMove == Piece.White ? score : -score;
        }
    }
}