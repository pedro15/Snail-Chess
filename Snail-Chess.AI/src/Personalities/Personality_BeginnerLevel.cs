using SnailChess.AI.Evaluation;
using SnailChess.AI.Search;

namespace SnailChess.AI.Personalities
{
    public partial struct BotPersonality
    {
        public static readonly BotPersonality BEGINNER = new BotPersonality()
        {
            /* ---------------------------- 
                Search
            ------------------------------- */
            searchOptions = new SearchOptions(_enable_qs: false, _enable_draw_detection: false, _enable_pvs: false, _enable_lmr: false, 
              _enable_nmp: false, _enable_see_pruning: false, _enable_lmp: false, _enable_razoring: false, _enable_rfp: false, _enable_iir: false, _enable_check_extension: false),
              
            /* ---------------------------- 
                Evaluation
            ------------------------------- */
            evaluationParams = new EvaluationParams(
                _material: new int[]
                {
                     0,
                     0,
                   -10,
                   -12,
                   -10,
                   -10,
                   -15,
                   -20,
                },
                _material_eg: new int []
                {
                     0,
                     0,
                     5,
                    -5,
                    -5,
                    -5,
                    -5,
                   -10,
                },
                _pst_pawn: new int[64]
                {  
                   0,   0,   0,   0,   0,   0,   0,   0,
                  20,  20,  20,  20,  20,  20,  20,  20,
                 -10, -10,  10,  20,  20,  10, -10, -10,
                   5,   5,   5,  15,  15,   5,   5,   5,
                   0,   0,   0,  10,  10,   0,   0,   0,
                   5,  -5, -10,   0,   0, -10,  -5,   5,
                   5,  10,  10, -20, -20,  10,  10,   5,
                   0,   0,   0,   0,   0,   0,   0,   0
                },
                _pst_pawn_eg: new int [64]
                {  
                   0,   0,   0,   0,   0,   0,   0,   0,
                  50,  50,  50,  50,  50,  50,  50,  50,
                  10,  10,  20,  30,  30,  20,  10,  10,
                   5,   5,  10,  25,  25,  10,   5,   5,
                   0,   0,   0,  20,  20,   0,   0,   0,
                   5,  -5, -10,   0,   0, -10,  -5,   5,
                   5,   5,  10, -10, -10,  10,   5,   5,
                   0,   0,   0,   0,   0,   0,   0,   0
                },
                _pst_knight: new int [64]
                {  
                  -20, -20, -20, -20, -20, -20, -20, -20,
                  -20, -25, -10, -10, -10, -10, -25, -20,
                  -20,   0,   0,   0,   0,   0,   0, -20,
                  -20,   5,   5,  10,  10,   5,   5, -20,
                  -20,   0,   5,  10,  10,   5,   0, -20,
                  -20,   5,  10,  15,  15,  10,   5, -20,
                  -20, -25,   0,   5,   5,   0, -25, -20,
                  -20, -20, -20, -20, -20, -20, -20, -20,
                },
                _pst_knight_eg: new int[64]
                { 
                  -20, -20, -20, -20, -20, -20, -20, -20,
                  -20, -25, -10, -10, -10, -10, -25, -20,
                  -20,   0,   0,   0,   0,   0,   0, -20,
                  -20,   5,   5,  12,  12,   5,   5, -20,
                  -20,   0,   5,  10,  10,   5,   0, -20,
                  -20,   5,  10,  15,  15,  10,   5, -20,
                  -20, -25,   0,   5,   5,   0, -25, -20,
                  -20, -20, -20, -20, -20, -20, -20, -20,
                },
                _pst_bishop: new int[64]
                { 
                  -20, -20, -20, -20, -20, -20, -20,  -20,
                  -20,  -5,   0,   0,   0,   0,  -5,  -20,
                  -20,  -5,   0,   0,   0,   0,  -5,  -20,
                  -20,  -5,   0,  15,  15,   0,  -5,  -20,
                  -20,   5,  10,  12,  12,  10,   5,  -20,
                  -20,   5,  10,  10,  10,  10,   5,  -20,
                  -20,  -5,   0,   0,   0,   0,  -5,  -20,
                  -20, -20, -20, -20, -20, -20, -20,  -20,
                },
                _pst_bishop_eg: new int[64] 
                {  
                  -30, -30, -30, -30, -30, -30, -30, -30,
                  -30,  -8,  -8,   0,   0,  -8,  -8, -30,
                  -30,   0,   0,   0,   0,   0,   0, -30,
                  -30,   5,  10,  10,  10,  10,   5, -30,
                  -30,  -8,  -8,  10,  10,  -8,  -8, -30,
                  -30,  -8,  -8,  12,  12,  -8,  -8, -30,
                  -30,  -8,  -8,  -8,  -8,  -8,  -8, -30,
                  -30, -30, -30, -30, -30, -30, -30, -30,
                },
                _pst_rook: new int[64] 
                {
                    0, -5, -5, -5, -5, -5, -5,   0,
                  -10,  0,  0,  0,  0,  0,  0, -10,
                  -10,  0,  0,  0,  0,  0,  0, -10,
                  -10,  0,  0,  0,  0,  0,  0, -10,
                  -10,  0,  0,  0,  0,  0,  0, -10,
                    5,  5,  5,  5,  5,  5,  5,   5,
                    5, 10, 10, 10, 10, 10, 10,   5,
                    5,  0,  0,  0,  0,  0,  0,   5,
                },
                _pst_rook_eg: new int[64] 
                {  
                    0, -5, -7, -7, -7, -5, -5,   0,
                  -10,  0, -5, -5, -5,  0,  0, -10,
                  -10,  0,  0,  0,  0,  0,  0, -10,
                  -10,  0,  0,  0,  0,  0,  0, -10,
                  -10,  5,  7,  7,  7,  5,  5, -10,
                    5, 10, 10, 10, 10, 10, 10,   5,
                    5,  5,  7,  7,  7,  5,  5,   5,
                    0,  0,  0,  0,  0,  0,  0,   0,
                },
                _pst_queen: new int[64] 
                { 
                  -25, -10, -10, -10, -10, -10, -10,  -25,
                  -10,  -5,   0,   0,   0,   0,  -5,  -10,
                  -10,  -5,   0,   0,   0,   5,  -5,  -10,
                  -10,   5,   5,   5,   5,   5,   5,  -10,
                  -10,   5,   5,   7,   7,   5,   5,  -10,
                  -10,  -5,   5,   5,   5,   5,  -5,  -10,
                  -10,  -5,   0,   0,   0,   0,  -5,  -10,
                  -25, -10, -10, -10, -10, -10, -10,  -25,
                },
                _pst_queen_eg: new int[64] 
                { 
                  -25, -20, -20, -20, -20, -20, -20,  -25,
                  -20, -15,   0,   0,   0,   0, -15,  -20,
                  -20, -10,   5,   5,   5,   5, -10,  -20,
                  -20, -10,   5,   7,   7,   5, -10,  -20,
                  -20, -10,   5,   7,   7,   5, -10,  -20,
                  -20, -10,   5,   5,   5,   5, -10,  -20,
                  -20, -15,   0,   0,   0,   0, -15,  -20,
                  -25, -20, -20, -20, -20, -20, -20,  -25,
                },
                _pst_king: new int[64] 
                {  
                  -30,   -40,  -40,  -50,  -50,  -40,  -40,  -30,
                  -30,   -40,  -40,  -50,  -50,  -40,  -40,  -30,
                  -30,   -40,  -40,  -50,  -50,  -40,  -40,  -30,
                  -30,   -40,  -40,  -50,  -50,  -40,  -40,  -30,
                  -20,   -30,  -30,  -40,  -40,  -30,  -30,  -20,
                  -10,   -20,  -10,  -10,  -10,  -10,  -10,  -10,
                   20,    20,    0,    0,    0,    0,   20,   20,
                   20,    30,    5,    2,    2,    5,   30,   20
                },
                _pst_king_eg: new int[64] 
                {  
                  -30,   -40,  -40,  -50,  -50,  -40,  -40,  -30,
                  -30,   -40,  -40,  -50,  -50,  -40,  -40,  -30,
                  -30,   -40,  -40,  -50,  -50,  -40,  -40,  -30,
                  -30,   -40,  -40,  -50,  -50,  -40,  -40,  -30,
                  -20,   -30,  -30,  -40,  -40,  -30,  -30,  -20,
                  -10,   -15,   -5,   -5,   -5,   -5,  -15,  -10,
                  -40,   -10,    0,    0,    0,    0,  -10,  -20,
                  -40,   -30,  -15,  -15,  -15,  -15,  -30,  -40
                },
                _passed_pawn_bonus: new int[] 
                {
                  0, 5, 5, 8, 8, 10, 12, 0
                },
                _passed_pawn_bonus_eg: new int[] 
                {
                  0, 5, 5, 8, 10, 15, 24, 0
                },
                _protected_passed_pawn_bonus: 5,
                _protected_passed_pawn_bonus_eg: 10,
                _double_pawn_penalty:    -5,
                _double_pawn_penalty_eg: -10,
                _isolated_pawn_penalty:   0,
                _tempo: 10,
                _kingSafety_attack_weights: new int[]
                {
                  0, 0, 2, 2, 2, 2, 0, 2
                },
                _bishop_mobility_score: 3,
                _rook_mobility_score:   3,
                _queen_mobility_score:  3,
                _bishop_mobility_score_eg: 0,
                _rook_mobility_score_eg:   0,
                _queen_mobility_score_eg:  0,
                _knight_pawn_adj: new int[]
                {
                  0, 0, 0, 0, 0, 0, 0, 0, 0
                },
                _rook_pawn_adj: new int[]
                {
                  0, 0, 0, 0, 0, 0, 0, 0, 0
                },
                _bishop_pair_score:     3,
                _bishop_pair_score_eg:  0,
                _rook_pair_score:       3,
                _rook_pair_score_eg:    0,
                _knight_pair_score:     5,
                _knight_pair_score_eg:  0
            )
        };
    }
}