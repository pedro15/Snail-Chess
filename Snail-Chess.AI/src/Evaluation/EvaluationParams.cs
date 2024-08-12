namespace SnailChess.AI.Evaluation
{
    [System.Serializable]
    public struct EvaluationParams
    {
        // General properties
        public int[] piece_material;
        public int[] piece_material_eg;

        // piece-square tables
        public int[] pst_pawn;
        public int[] pst_pawn_eg;
        
        public int[] pst_knight;
        public int[] pst_knight_eg;
        
        public  int[] pst_bishop;
        public  int[] pst_bishop_eg;
        
        public int[] pst_rook;
        public int[] pst_rook_eg;
        
        public int[] pst_queen;
        public int[] pst_queen_eg;

        public int[] pst_king;
        public int[] pst_king_eg;

        // Pawn evaluation
        public int[] passed_pawn_bonus;
        public int[] passed_pawn_bonus_eg;
        public short protected_passed_pawn_bonus;
        public short protected_passed_pawn_bonus_eg;
        public short double_pawn_penalty;
        public short double_pawn_penalty_eg;
        public short isolated_pawn_penalty;

        // tempo bonus
        public short tempo;

        // King safety
        public int[] king_safety_attack_weights;

        // mobility
        public short bishop_mobility_score;
        public short rook_mobility_score;
        public short queen_mobility_score;

        public short bishop_mobility_score_eg;
        public short rook_mobility_score_eg;
        public short queen_mobility_score_eg;
        // material balance
        public int[] knight_pawn_adj;
        public int[] rook_pawn_adj;

        public short bishop_pair_score;
        public short bishop_pair_score_eg;

        public short rook_pair_score;
        public short rook_pair_score_eg;

        public short knight_pair_score;
        public short knight_pair_score_eg;

        public EvaluationParams(int[] _material, int[] _pst_pawn, int[] _pst_knight, int[] _pst_bishop, int[] _pst_rook, int[] _pst_queen, int[] _pst_king,
            int[] _material_eg, int[] _pst_pawn_eg, int[] _pst_knight_eg, int[] _pst_bishop_eg, int[] _pst_rook_eg, int[] _pst_queen_eg, int[] _pst_king_eg,
            int[] _passed_pawn_bonus, int[] _passed_pawn_bonus_eg, short _protected_passed_pawn_bonus, short _protected_passed_pawn_bonus_eg, 
            short _isolated_pawn_penalty, short _double_pawn_penalty, short _double_pawn_penalty_eg, short _tempo, 
            int[] _kingSafety_attack_weights, short _bishop_mobility_score, short _rook_mobility_score, short _queen_mobility_score, 
                short _bishop_mobility_score_eg, short _rook_mobility_score_eg, short _queen_mobility_score_eg, int[] _knight_pawn_adj, int[] _rook_pawn_adj,
                short _bishop_pair_score, short _rook_pair_score, short _knight_pair_score, short _bishop_pair_score_eg, short _rook_pair_score_eg, short _knight_pair_score_eg)
        {
            piece_material = _material;
            piece_material_eg = _material_eg;

            pst_pawn = _pst_pawn;
            pst_pawn_eg  = _pst_pawn_eg;

            pst_knight = _pst_knight;
            pst_knight_eg = _pst_knight_eg;

            pst_bishop = _pst_bishop;
            pst_bishop_eg = _pst_bishop_eg;
            
            pst_rook = _pst_rook;
            pst_rook_eg = _pst_rook_eg;

            pst_queen = _pst_queen;
            pst_queen_eg = _pst_queen_eg;

            pst_king = _pst_king;
            pst_king_eg = _pst_king_eg;

            passed_pawn_bonus = _passed_pawn_bonus;
            passed_pawn_bonus_eg = _passed_pawn_bonus_eg;
            protected_passed_pawn_bonus = _protected_passed_pawn_bonus;
            protected_passed_pawn_bonus_eg = _protected_passed_pawn_bonus_eg;
            isolated_pawn_penalty = _isolated_pawn_penalty;
            double_pawn_penalty = _double_pawn_penalty;
            double_pawn_penalty_eg = _double_pawn_penalty_eg;
            tempo = _tempo;
            king_safety_attack_weights = _kingSafety_attack_weights;

            bishop_mobility_score = _bishop_mobility_score;
            bishop_mobility_score_eg = _bishop_mobility_score_eg;

            rook_mobility_score = _rook_mobility_score;
            rook_mobility_score_eg = _rook_mobility_score_eg;

            queen_mobility_score = _queen_mobility_score;
            queen_mobility_score_eg = _queen_mobility_score_eg;

            knight_pawn_adj = _knight_pawn_adj;
            rook_pawn_adj = _rook_pawn_adj;
            
            bishop_pair_score = _bishop_pair_score;
            bishop_pair_score_eg = _bishop_pair_score_eg;

            rook_pair_score = _rook_pair_score;
            rook_pair_score_eg = _rook_pair_score_eg;

            knight_pair_score = _knight_pair_score;
            knight_pair_score_eg = _knight_pair_score_eg;
        }
    }
}