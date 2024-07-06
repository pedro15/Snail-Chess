using System.Collections.Generic;
using System.Runtime.CompilerServices;

using static SnailChess.AI.Evaluation.EvaluationUtils;
using SnailChess.AI.Evaluation;
using SnailChess.Core;
using SnailChess.Tuner.Core;
using SnailChess.Tuner.Data;
using SnailChess.Core.MoveGen;

namespace SnailChess.Tuner
{
    internal static class TunerEvalImpl
    {
        private static readonly string[] piece_key_names = {"black", "white", "pawn" , "knight", "bishop", "rook", "queen", "king"};

        private const string KEY_PASSED_PAWN = "passed_pawn_common";
        private const string KEY_PROTECTED_PASSED_PAWN = "protected_passed_pawn";
        private const string KEY_PROTECTED_PASSED_PAWN_EG = "protected_passed_pawn_EG";

        private const string KEY_ISOLATED_PAWN = "isolated_pawn_penalty";
        private const string KEY_DOUBLED_PAWN = "double_pawn_penalty";
        private const string KEY_DOUBLED_PAWN_EG = "double_pawn_penalty_EG";
        private const string KEY_TEMPO_BONUS = "TEMPO_BONUS";
        private const string KEY_KING_SAFETY_WEIGHTS = "KING_SAFETY_WEIGHTS";
        
        private const string KEY_BISHOP_MOBILITY_SCORE = "BISHOP_MOBILITY_SCORE";
        private const string KEY_BISHOP_MOBILITY_SCORE_EG = "BISHOP_MOBILITY_SCORE_EG";

        private const string KEY_ROOK_MOBILITY_SCORE = "ROOK_MOBILITY_SCORE";
        private const string KEY_ROOK_MOBILITY_SCORE_EG = "ROOK_MOBILITY_SCORE_EG";

        private const string KEY_QUEEN_MOBILITY_SCORE = "QUEEN_MOBILITY_SCORE";
        private const string KEY_QUEEN_MOBILITY_SCORE_EG = "QUEEN_MOBILITY_SCORE_EG";

        private const string KEY_BISHOP_PAIR_SCORE = "BISHOP_PAIR_SCORE";
        private const string KEY_BISHOP_PAIR_SCORE_EG = "BISHOP_PAIR_SCORE_EG";

        private const string KEY_ROOK_PAIR_SCORE = "ROOK_PAIR_SCORE";
        private const string KEY_ROOK_PAIR_SCORE_EG = "ROOK_PAIR_SCORE_EG";

        private const string KEY_KNIGHT_PAIR_SCORE = "KNIGHT_PAIR_SCORE";
        private const string KEY_KNIGHT_PAIR_SCORE_EG = "KNIGHT_PAIR_SCORE_EG";

        private const string KEY_KNIGHT_PAWN_ADJ = "KNIGHT_PAWN_ADJ";
        private const string KEY_ROOK_PAWN_ADJ = "ROOK_PAWN_ADJ";


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ParsePieceTypeFromName(string _name)
        {
            if (_name.Contains("pawn"))
                return Piece.Pawn;
            else if (_name.Contains("knight"))
                return Piece.Knight;
            else if (_name.Contains("bishop"))
                return Piece.Bishop;
            else if (_name.Contains("rook"))
                return Piece.Rook;
            else if (_name.Contains("queen"))
                return Piece.Queen;
            else if (_name.Contains("king"))
                return Piece.King;
            
            return Piece.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ExtractEvalFeatures(in ChessBoard _board, in Dictionary<string,int> _indexes_db, out TunerParamData[] _paramsData, 
            out byte _gamePhase, out byte _sideToMove)
        {

            Dictionary<string,TunerParamData> db = new Dictionary<string, TunerParamData>();
            ulong bb_piece, bb_own, piece_attacks, opponent_king_zone, opponent_ray_mask;
            byte sq, sq_pst, opponent_king_sq, count_set, pawn_count, bishop_count, rook_count, knight_count;
            int game_phase = 0, index;
            bool is_white;

            for (byte color = Piece.Black; color <= Piece.White; color++)
            {
                is_white = color == Piece.White;
                
                // init eval data for this color
                pawn_count = 0;
                knight_count = 0;
                bishop_count = 0;
                rook_count = 0;

                opponent_king_sq = BitUtils.BitScan(_board.bitboards[Piece.King] & _board.bitboards[Piece.FlipColor(color)]);
                opponent_king_zone = King_zone[opponent_king_sq];
                opponent_ray_mask = ~GetAttackMap(in _board, Piece.FlipColor(color));

                for (byte piece = Piece.Pawn; piece <= Piece.King; piece++)
                {
                    bb_piece = _board.bitboards[color] & _board.bitboards[piece];
                    bb_own = bb_piece;

                    while(bb_piece != 0)
                    {
                        sq = BitUtils.BitScan(bb_piece);
                        sq_pst = is_white ? BoardUtils.SquareInvert(sq) : sq;

                        // Update Game Phase
                        game_phase += GAME_PHASE_VALUES[piece];

                        // PST MG
                        TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_PST(piece, sq_pst, PHASE_TYPE.MG), color);
                        // PST EG
                        TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_PST(piece, sq_pst, PHASE_TYPE.EG), color);

                        // MATERIAL MG
                        TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Material(piece, PHASE_TYPE.MG), color);
                        // MATERIAL EG 
                        TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Material(piece, PHASE_TYPE.EG), color);

                        switch(piece)
                        {
                            // Pawn evaluation
                            case Piece.Pawn:

                            pawn_count++;
                            
                            // Doubled Pawn
                            if (PawnStructureUtil.IsPawnDoubled(sq, in bb_own))
                            {
                                TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_DOUBLED_PAWN, color);
                                TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_DOUBLED_PAWN_EG, color);
                            }

                            // Isolated Pawns
                            if (PawnStructureUtil.IsPawnIsolated(sq, in bb_own))
                            {
                                TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_ISOLATED_PAWN, color);
                            }
                            
                            // Passed Pawn, apply bonus
                            if (PawnStructureUtil.IsPawnPassed(sq, color, in _board.bitboards[Piece.Pawn], in _board.bitboards[Piece.FlipColor(color)]))
                            {
                                index = is_white ? BoardUtils.SquareToRank(sq) : BoardUtils.SquareToRank(BoardUtils.SquareInvert(sq));

                                // normal passer
                                TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Array(KEY_PASSED_PAWN, 
                                PHASE_TYPE.MG, index), color);

                                TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Array(KEY_PASSED_PAWN, 
                                PHASE_TYPE.EG, index), color);

                                // protected passed pawn
                                if (PawnStructureUtil.IsPawnProtected(sq, color, in bb_own))
                                {
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_PROTECTED_PASSED_PAWN, color);
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_PROTECTED_PASSED_PAWN_EG, color);
                                }
                            }

                            break;

                            case Piece.Knight:

                                knight_count++;
                                
                                piece_attacks = MoveGenerator.KnightMoves(sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Array(KEY_KING_SAFETY_WEIGHTS, PHASE_TYPE.GL, Piece.Knight), 
                                        color, BitUtils.Count(opponent_king_zone & piece_attacks));
                                }

                                // knight material adjust
                                TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Array(KEY_KNIGHT_PAWN_ADJ, PHASE_TYPE.GL, pawn_count), color);

                            break;

                            case Piece.Bishop:

                                bishop_count++;

                                piece_attacks = MoveGenerator.BishopMoves(_board.Occupancy, sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Array(KEY_KING_SAFETY_WEIGHTS, PHASE_TYPE.GL, Piece.Bishop), 
                                        color, BitUtils.Count(opponent_king_zone & piece_attacks));
                                }

                                // Bishop mobility
                                if (piece_attacks != 0)
                                {
                                    count_set = BitUtils.Count(piece_attacks);
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_BISHOP_MOBILITY_SCORE, color, count_set);
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_BISHOP_MOBILITY_SCORE_EG, color, count_set);
                                }
                            break;

                            case Piece.Rook:
                                
                                rook_count++;

                                piece_attacks = MoveGenerator.RookMoves(_board.Occupancy, sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Array(KEY_KING_SAFETY_WEIGHTS, PHASE_TYPE.GL, Piece.Rook), 
                                        color, BitUtils.Count(opponent_king_zone & piece_attacks));
                                }

                                // Rook mobility
                                if (piece_attacks != 0)
                                {
                                    count_set = BitUtils.Count(piece_attacks);
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_ROOK_MOBILITY_SCORE, color, count_set);
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_ROOK_MOBILITY_SCORE_EG, color, count_set);
                                }

                                // rook material adjust
                                TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Array(KEY_ROOK_PAWN_ADJ, PHASE_TYPE.GL, pawn_count), color);

                            break;

                            case Piece.Queen:
                                piece_attacks = MoveGenerator.QueenMoves(_board.Occupancy, sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Array(KEY_KING_SAFETY_WEIGHTS, PHASE_TYPE.GL, Piece.Queen), 
                                        color, BitUtils.Count(opponent_king_zone & piece_attacks));
                                }
                                
                                // Queen mobility
                                if (piece_attacks != 0)
                                {
                                    count_set = BitUtils.Count(piece_attacks);
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_QUEEN_MOBILITY_SCORE, color, count_set);
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_QUEEN_MOBILITY_SCORE_EG, color, count_set);
                                }
                            break;

                            case Piece.King:
                                piece_attacks = MoveGenerator.KingMoves(sq) & opponent_ray_mask;
                                if ((piece_attacks & opponent_king_zone) != 0)
                                {
                                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, GetKey_Array(KEY_KING_SAFETY_WEIGHTS, PHASE_TYPE.GL, Piece.King), 
                                        color, BitUtils.Count(opponent_king_zone & piece_attacks));
                                }
                            break;
                        }

                        bb_piece = BitUtils.PopLsb(bb_piece);
                    }
                }

                // bishop pair
                if (bishop_count > 1)
                {
                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_BISHOP_PAIR_SCORE, color);
                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_BISHOP_PAIR_SCORE_EG, color);
                }
                // rook pair
                if (rook_count > 1)
                {
                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_ROOK_PAIR_SCORE, color);
                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_ROOK_PAIR_SCORE_EG, color);
                }
                // knight pair
                if (knight_count > 1)
                {
                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_KNIGHT_PAIR_SCORE, color);
                    TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_KNIGHT_PAIR_SCORE_EG, color);
                }
            }

            // Tempo bonus
            TunerEvaluator.AddTunerParamFeature(ref db, in _indexes_db, KEY_TEMPO_BONUS, _board.sideToMove);
            
            _gamePhase = (byte)game_phase;
            _sideToMove = _board.sideToMove;

            _paramsData = new TunerParamData[db.Values.Count];
            db.Values.CopyTo(_paramsData, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TunerParam[] ExtractParamsFromEval(in EvaluationParams _params, out Dictionary<string,int> _indexesdb)
        {
            List<TunerParam> param_list = new List<TunerParam>();

            // ----- Add PST MG -------
            // Pawn
            param_list.AddRange(TunerParam_PST(in _params.pst_pawn,   Piece.Pawn,   PHASE_TYPE.MG));
            // Knight
            param_list.AddRange(TunerParam_PST(in _params.pst_knight, Piece.Knight, PHASE_TYPE.MG));
            // Bishop
            param_list.AddRange(TunerParam_PST(in _params.pst_bishop, Piece.Bishop, PHASE_TYPE.MG));
            // Rook
            param_list.AddRange(TunerParam_PST(in _params.pst_rook,   Piece.Rook,   PHASE_TYPE.MG));
            // Queen
            param_list.AddRange(TunerParam_PST(in _params.pst_queen,  Piece.Queen,  PHASE_TYPE.MG));
            // King
            param_list.AddRange(TunerParam_PST(in _params.pst_king,  Piece.King,  PHASE_TYPE.MG));
            // Add Material MG
            param_list.AddRange(TunerParam_Material(in _params.piece_material, PHASE_TYPE.MG));
            // ----- Add PST EG ------
            // Pawn
            param_list.AddRange(TunerParam_PST(in _params.pst_pawn_eg,   Piece.Pawn,   PHASE_TYPE.EG));
            // Knight
            param_list.AddRange(TunerParam_PST(in _params.pst_knight_eg, Piece.Knight, PHASE_TYPE.EG));
            // Bishop
            param_list.AddRange(TunerParam_PST(in _params.pst_bishop_eg, Piece.Bishop, PHASE_TYPE.EG));
            // Rook
            param_list.AddRange(TunerParam_PST(in _params.pst_rook_eg,   Piece.Rook,   PHASE_TYPE.EG));
            // Queen
            param_list.AddRange(TunerParam_PST(in _params.pst_queen_eg,  Piece.Queen,  PHASE_TYPE.EG));
            // King 
            param_list.AddRange(TunerParam_PST(in _params.pst_king_eg,  Piece.King,  PHASE_TYPE.EG));
            // Add Material MG
            param_list.AddRange(TunerParam_Material(in _params.piece_material_eg, PHASE_TYPE.EG));
            
            // ------- Pawn structure -----------
            // Passed pawn
            param_list.AddRange(TunerParam_Array(in _params.passed_pawn_bonus, KEY_PASSED_PAWN, PHASE_TYPE.MG));
            param_list.AddRange(TunerParam_Array(in _params.passed_pawn_bonus_eg, KEY_PASSED_PAWN, PHASE_TYPE.EG));
            // Protected passed pawn
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.MG, KEY_PROTECTED_PASSED_PAWN, _params.protected_passed_pawn_bonus));
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.EG, KEY_PROTECTED_PASSED_PAWN_EG, _params.protected_passed_pawn_bonus_eg));
            // Doubled pawns
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.MG, KEY_DOUBLED_PAWN, _params.double_pawn_penalty));
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.EG, KEY_DOUBLED_PAWN_EG, _params.double_pawn_penalty_eg));
            // Isolated pawn
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.GL, KEY_ISOLATED_PAWN, _params.isolated_pawn_penalty));

            // ---------- Global Eval Params -----------
            // Tempo
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.MG, KEY_TEMPO_BONUS, _params.tempo));
            // king safety table
            param_list.AddRange(TunerParam_Array(_params.king_safety_attack_weights, KEY_KING_SAFETY_WEIGHTS, PHASE_TYPE.GL));

            // Bishop mobility score
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.MG, KEY_BISHOP_MOBILITY_SCORE, _params.bishop_mobility_score));
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.EG, KEY_BISHOP_MOBILITY_SCORE_EG, _params.bishop_mobility_score_eg));
            // Rook mobility score
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.MG, KEY_ROOK_MOBILITY_SCORE, _params.rook_mobility_score));
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.EG, KEY_ROOK_MOBILITY_SCORE_EG, _params.rook_mobility_score_eg));
            // Queen mobility score
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.MG, KEY_QUEEN_MOBILITY_SCORE, _params.queen_mobility_score));
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.EG, KEY_QUEEN_MOBILITY_SCORE_EG, _params.queen_mobility_score_eg));

            // Knight Pawn adj
            param_list.AddRange(TunerParam_Array(in _params.knight_pawn_adj, KEY_KNIGHT_PAWN_ADJ, PHASE_TYPE.GL));
            // Rook Pawn adj
            param_list.AddRange(TunerParam_Array(in _params.rook_pawn_adj, KEY_ROOK_PAWN_ADJ, PHASE_TYPE.GL));

            // Bishop pair
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.MG, KEY_BISHOP_PAIR_SCORE, _params.bishop_pair_score));
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.EG, KEY_BISHOP_PAIR_SCORE_EG, _params.bishop_pair_score_eg));
            // Knight pair
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.MG, KEY_KNIGHT_PAIR_SCORE, _params.knight_pair_score));
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.EG, KEY_KNIGHT_PAIR_SCORE_EG, _params.knight_pair_score_eg));
            // Rook pair
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.MG, KEY_ROOK_PAIR_SCORE, _params.rook_pair_score));
            param_list.Add(new TunerParam(PARAM_TYPE.DEFAULT, PHASE_TYPE.EG, KEY_ROOK_PAIR_SCORE_EG, _params.rook_pair_score_eg));

            _indexesdb = new Dictionary<string, int>();
            for (int i = 0; i < param_list.Count; i++)
            {
				if (!param_list[i].IsValid) continue;
                _indexesdb.Add(param_list[i].name, i);
            }

            return param_list.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EvaluationParams TransformParamsToEval(in TunerParam[] _params)
        {
            int[] piece_material = new int[8];
            int[] piece_material_eg = new int[8];
            int[] pst_pawn = new int[64];
            int[] pst_pawn_eg = new int[64];
            int[] pst_knight = new int[64];
            int[] pst_knight_eg = new int[64];
            int[] pst_bishop = new int[64];
            int[] pst_bishop_eg = new int[64];
            int[] pst_rook = new int[64];
            int[] pst_rook_eg = new int[64];
            int[] pst_queen = new int[64];
            int[] pst_queen_eg = new int[64];
            int[] pst_king = new int[64];
            int[] pst_king_eg = new int[64];
            int[] passed_pawn_bonus = new int[8];
            int[] passed_pawn_bonus_eg = new int[8];
            int[] king_safety_weights = new int[8];
            int[] knight_pawn_adj = new int[9];
            int[] rook_pawn_adj = new int[9];
            short protected_passed_pawn_bonus = 0; 
            short protected_passed_pawn_bonus_eg = 0;
            short isolated_pawn_penalty = 0;
            short double_pawn_penalty = 0;
            short double_pawn_penalty_eg = 0;
            short tempo_bonus = 0;
            short bishop_mobility_score = 0;
            short bishop_mobility_score_eg = 0;
            short rook_mobility_score = 0;
            short rook_mobility_score_eg = 0;
            short queen_mobility_score = 0;
            short queen_mobility_score_eg = 0;
            short bishop_pair_score = 0;
            short bishop_pair_score_eg = 0;
            short knight_pair_score = 0;
            short knight_pair_score_eg = 0;
            short rook_pair_score = 0;
            short rook_pair_score_eg = 0;

            TunerParam curr_param;
            int index;
            byte piece_type;
            bool is_eg;
            
            for (int i = 0; i < _params.Length; i++)
            {
                curr_param = _params[i];
				if (!curr_param.IsValid) continue;

                is_eg = _params[i].phase == PHASE_TYPE.EG;

                // Load PST
                if (curr_param.Match("pst_" , PARAM_TYPE.PST))
                {
                    index = curr_param.array_index;
                    piece_type = ParsePieceTypeFromName(curr_param.name);

                    switch(piece_type)
                    {
                        case Piece.Pawn:
                            if (is_eg)
                            {
                                pst_pawn_eg[index] = curr_param.value;
                            }else 
                            {
                                pst_pawn[index] = curr_param.value;
                            }
                        break;

                        case Piece.Knight:
                            if (is_eg)
                            {
                                pst_knight_eg[index] = curr_param.value;
                            }else 
                            {
                                pst_knight[index] = curr_param.value;
                            }
                        break;

                        case Piece.Bishop:
                            if (is_eg)
                            {
                                pst_bishop_eg[index] = curr_param.value;
                            }else 
                            {
                                pst_bishop[index] = curr_param.value;
                            }
                        break;

                        case Piece.Rook:
                            if (is_eg)
                            {   
                                pst_rook_eg[index] = curr_param.value;
                            }else 
                            {
                                pst_rook[index] = curr_param.value;
                            }
                        break;

                        case Piece.Queen:
                            if (is_eg)
                            {
                                pst_queen_eg[index] = curr_param.value;
                            }else 
                            {
                                pst_queen[index] = curr_param.value;
                            }
                        break;

                        case Piece.King:
                            if (is_eg)
                            {
                                pst_king_eg[index] = curr_param.value;
                            }else 
                            {
                                pst_king[index] = curr_param.value;
                            }
                        break;
                    }
                }
                // Load Material
                else if (curr_param.Match("material_" , PARAM_TYPE.ARRAY))
                {
                    piece_type = ParsePieceTypeFromName(curr_param.name);
                    switch (piece_type)
                    {
                        case Piece.Pawn:
                            if (is_eg)
                            {
                                piece_material_eg[Piece.Pawn] = curr_param.value;
                            }else 
                            {
                                piece_material[Piece.Pawn] = curr_param.value;
                            }
                        break;

                        case Piece.Knight:
                            if (is_eg)
                            {
                                piece_material_eg[Piece.Knight] = curr_param.value;
                            }else 
                            {
                                piece_material[Piece.Knight] = curr_param.value;
                            }
                        break;

                        case Piece.Bishop:
                            if (is_eg)
                            {
                                piece_material_eg[Piece.Bishop] = curr_param.value;
                            }else 
                            {
                                piece_material[Piece.Bishop] = curr_param.value;
                            }
                        break;

                        case Piece.Rook:
                            if (is_eg)
                            {
                                piece_material_eg[Piece.Rook] = curr_param.value;
                            }else 
                            {
                                piece_material[Piece.Rook] = curr_param.value;
                            }
                        break;

                        case Piece.Queen:
                            if (is_eg)
                            {
                                piece_material_eg[Piece.Queen] = curr_param.value;
                            }else 
                            {
                                piece_material[Piece.Queen] = curr_param.value;
                            }
                        break;

                        case Piece.King:
                            if (is_eg)
                            {
                                piece_material_eg[Piece.King] = curr_param.value;
                            }else 
                            {
                                piece_material[Piece.King] = curr_param.value;
                            }
                        break;  
                    }
                }
                // Passed pawn bonus 
                else if (curr_param.Match(KEY_PASSED_PAWN , PARAM_TYPE.ARRAY))
                {
                    index = curr_param.array_index;
                    if (is_eg)
                    {
                        passed_pawn_bonus_eg[index] = curr_param.value;
                    }else 
                    {
                        passed_pawn_bonus[index] = curr_param.value;
                    }
                }
                // Protected passed pawn
                else if (curr_param.Match(KEY_PROTECTED_PASSED_PAWN, PARAM_TYPE.DEFAULT))
                {
                    if (is_eg)
                    {
                        protected_passed_pawn_bonus_eg = (short)curr_param.value;
                    }else 
                    {
                        protected_passed_pawn_bonus = (short)curr_param.value;
                    }
                }
                // isolated pawn penalty
                else if (curr_param.Match(KEY_ISOLATED_PAWN , PARAM_TYPE.DEFAULT))
                {
                    isolated_pawn_penalty = (short)curr_param.value;
                }
                // Doubled pawn
                else if (curr_param.Match(KEY_DOUBLED_PAWN, PARAM_TYPE.DEFAULT))
                {
                    if (is_eg)
                    {
                        double_pawn_penalty_eg = (short)curr_param.value;
                    }else 
                    {
                        double_pawn_penalty = (short)curr_param.value;
                    }
                }
                // Tempo
                else if (curr_param.Match(KEY_TEMPO_BONUS, PARAM_TYPE.DEFAULT))
                {
                    tempo_bonus = (short)curr_param.value;
                }
                // king safety weights table
                else if (curr_param.Match(KEY_KING_SAFETY_WEIGHTS, PARAM_TYPE.ARRAY))
                {
                    king_safety_weights[curr_param.array_index] = curr_param.value;
                }
                // Bishop mobility score
                else if(curr_param.Match(KEY_BISHOP_MOBILITY_SCORE, PARAM_TYPE.DEFAULT))
                {
                    if (is_eg)
                        bishop_mobility_score_eg = (short)curr_param.value;
                    else 
                        bishop_mobility_score = (short)curr_param.value;
                }
                // Rook mobility score
                else if (curr_param.Match(KEY_ROOK_MOBILITY_SCORE, PARAM_TYPE.DEFAULT))
                {
                    if (is_eg)
                        rook_mobility_score_eg = (short)curr_param.value;
                    else 
                        rook_mobility_score = (short)curr_param.value;
                }
                // Queen mobility score
                else if (curr_param.Match(KEY_QUEEN_MOBILITY_SCORE, PARAM_TYPE.DEFAULT))
                {
                    if (is_eg)
                        queen_mobility_score_eg = (short)curr_param.value;
                    else 
                        queen_mobility_score = (short)curr_param.value;
                }
                // Knight Pawn adj
                else if (curr_param.Match(KEY_KNIGHT_PAWN_ADJ, PARAM_TYPE.ARRAY))
                {
                    knight_pawn_adj[curr_param.array_index] = curr_param.value;
                }
                // Rook Pawn adj
                else if (curr_param.Match(KEY_ROOK_PAWN_ADJ, PARAM_TYPE.ARRAY))
                {
                    rook_pawn_adj[curr_param.array_index] = curr_param.value;
                }
                // Bishop Pair
                else if (curr_param.Match(KEY_BISHOP_PAIR_SCORE, PARAM_TYPE.DEFAULT))
                {
                    if (is_eg)
                        bishop_pair_score_eg = (short)curr_param.value;
                    else 
                        bishop_pair_score = (short)curr_param.value;

                }
                // Rook Pair
                else if (curr_param.Match(KEY_ROOK_PAIR_SCORE, PARAM_TYPE.DEFAULT))
                {
                    if (is_eg)
                        rook_pair_score_eg = (short)curr_param.value;
                    else 
                        rook_pair_score = (short)curr_param.value;
                }
                // Knight Pair
                else if (curr_param.Match(KEY_KNIGHT_PAIR_SCORE, PARAM_TYPE.DEFAULT))
                {
                    if (is_eg)
                        knight_pair_score_eg = (short)curr_param.value;
                    else 
                        knight_pair_score = (short)curr_param.value;
                }
            }
            
            return new EvaluationParams(
                    _material: piece_material, 
                    _pst_pawn: pst_pawn, 
                    _pst_knight: pst_knight, 
                    _pst_bishop: pst_bishop, 
                    _pst_rook: pst_rook, 
                    _pst_queen: pst_queen, 
                    _pst_king: pst_king, 
                    _material_eg: piece_material_eg, 
                    _pst_pawn_eg: pst_pawn_eg,
                    _pst_knight_eg: pst_knight_eg, 
                    _pst_bishop_eg: pst_bishop_eg, 
                    _pst_rook_eg: pst_rook_eg, 
                    _pst_queen_eg: pst_queen_eg, 
                    _pst_king_eg: pst_king_eg, 
                    _passed_pawn_bonus: passed_pawn_bonus, 
                    _passed_pawn_bonus_eg: passed_pawn_bonus_eg, 
                    _protected_passed_pawn_bonus: protected_passed_pawn_bonus, 
                    _protected_passed_pawn_bonus_eg: protected_passed_pawn_bonus_eg, 
                    _isolated_pawn_penalty: isolated_pawn_penalty, 
                    _double_pawn_penalty: double_pawn_penalty, 
                    _double_pawn_penalty_eg: double_pawn_penalty_eg, 
                    _tempo: tempo_bonus,
                    _kingSafety_attack_weights: king_safety_weights,
                    _bishop_mobility_score: bishop_mobility_score,
                    _bishop_mobility_score_eg: bishop_mobility_score_eg,
                    _rook_mobility_score: rook_mobility_score,
                    _rook_mobility_score_eg: rook_mobility_score_eg,
                    _queen_mobility_score: queen_mobility_score,
                    _queen_mobility_score_eg: queen_mobility_score_eg,
                    _knight_pawn_adj: knight_pawn_adj,
                    _rook_pawn_adj: rook_pawn_adj,
                    _bishop_pair_score: bishop_pair_score,
                    _bishop_pair_score_eg: bishop_pair_score_eg,
                    _rook_pair_score: rook_pair_score,
                    _rook_pair_score_eg: rook_pair_score_eg,
                    _knight_pair_score: knight_pair_score,
                    _knight_pair_score_eg: knight_pair_score_eg);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetKey_PST(byte _piece, byte _square, PHASE_TYPE _phase)
        {
            return $"pst_{piece_key_names[_piece]}_{_phase}_{_square}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]    
        internal static string GetKey_Material(byte _piece, PHASE_TYPE _phase)
        {
            return $"material_{piece_key_names[_piece]}_{_phase}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]  
        internal static string GetKey_Array(string _arrayName, PHASE_TYPE _phase, int _array_index)
        {
            return $"{_arrayName}_{_phase}_{_array_index}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TunerParam[] TunerParam_PST(in int[] _pst, byte _piece, PHASE_TYPE _phase)
        {
            TunerParam[] pst_params = new TunerParam[64];

            for (byte sq = 0; sq < 64; sq++)
            {
                if (_piece == Piece.Pawn && (BoardUtils.SquareToRank(sq) == 7 || BoardUtils.SquareToRank(sq) == 0)) continue;
                pst_params[sq] = new TunerParam(PARAM_TYPE.PST, _phase, GetKey_PST(_piece, sq, _phase), _pst[sq], sq);
            }
            return pst_params;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TunerParam[] TunerParam_Material(in int[] _material, PHASE_TYPE _phase)
        {
            TunerParam[] material_params = new TunerParam[8];
            for (byte piece = Piece.Pawn; piece <= Piece.King; piece++)
            {
                material_params[piece] = new TunerParam(PARAM_TYPE.ARRAY, _phase, GetKey_Material(piece, _phase), _material[piece], piece);
            }
            return material_params;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TunerParam[] TunerParam_Array(in int[] _array, string _arr_name, PHASE_TYPE _phase)
        {
            TunerParam[] arr_params = new TunerParam[_array.Length];
            for (int i = 0; i < _array.Length; i++)
            {
                arr_params[i] = new TunerParam(PARAM_TYPE.ARRAY, _phase, GetKey_Array(_arr_name, _phase, i), _array[i], i);
            }
            return arr_params;
        }

    }
}