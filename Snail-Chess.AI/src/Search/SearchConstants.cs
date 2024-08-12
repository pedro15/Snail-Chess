namespace SnailChess.AI.Search
{
    internal static class SearchConstants 
    {
        /* ---------------------------------------------------------------
            MVV/LVA (most valable victim / least valuable attacker) Table
            [Attackers] Pawn Knight Bishop   Rook  Queen   King [Victims]
                Pawn    105    205    305    405    505    605
              Knight    104    204    304    404    504    604
              Bishop    103    203    303    403    503    603
                Rook    102    202    302    402    502    602
               Queen    101    201    301    401    501    601
                King    100    200    300    400    500    600
        ---------------------------------------------------------------- */
        // [attacker][victim]
        internal static readonly short[][] MVV_LVA = new short[8][]
        {
                new short[]{}, // white_player: not used
                new short[]{}, // black_player: not useed
                // Pawn
                new short[] 
                {
                    000,   // -> not used (white)
                    000,   // -> not used (black)
                    105,   // -> pawn
                    205,   // -> knight
                    305,   // -> bishop 
                    405,   // -> rook 
                    505,   // -> queen 
                    605    // -> king
                },
                // Knight
                new short[] 
                {
                    000,   // -> not used (white)
                    000,   // -> not used (black)
                    104,   // -> pawn
                    204,   // -> knight
                    304,   // -> bishop 
                    404,   // -> rook 
                    504,   // -> queen 
                    604    // -> king
                },
                // Bishop
                new short[]
                {
                    000,   // -> not used (white)
                    000,   // -> not used (black)
                    103,   // -> pawn
                    203,   // -> knight
                    303,   // -> bishop 
                    403,   // -> rook 
                    503,   // -> queen 
                    603    // -> king
                },
                // Rook
                new short[]
                {
                    000,   // -> not used (white)
                    000,   // -> not used (black)
                    102,   // -> pawn
                    202,   // -> knight
                    302,   // -> bishop 
                    402,   // -> rook 
                    502,   // -> queen 
                    602    // -> king
                },
                // Queen
                new short[]
                {
                    000,   // -> not used (white)
                    000,   // -> not used (black)
                    101,   // -> pawn
                    201,   // -> knight
                    301,   // -> bishop 
                    401,   // -> rook 
                    501,   // -> queen 
                    601    // -> king
                },
                // King
                new short[]
                {
                    000,   // -> not used (white)
                    000,   // -> not used (black)
                    100,   // -> pawn
                    200,   // -> knight
                    300,   // -> bishop 
                    400,   // -> rook 
                    500,   // -> queen 
                    600    // -> king
                }
        };
        
        public static readonly short[] SEE_PIECE_VALUES = 
        {
            0,     // white
            0,     // black
            100,   // pawn
            300,   // knight
            300,   // bishop
            500,   // rook
            900,   // queen
            15000, // king
            0      // No-piece
        };

        internal const short VALUE_MATE_SCORE = 28000;
        internal const short VALUE_MATE = 29000;
        internal const short VALUE_INFINITY = 30000;

        internal const short LMR_DEPTH = 3;
        internal const short LMR_MOVE_COUNT = 4;

        internal const short SORTING_SCORE_TTMOVE             = 32700;
        internal const short SORTING_SCORE_PROMOTION          = 25000;
        internal const short SORTING_SCORE_GOOD_CAPTURES      = 20000;
        internal const short GOOD_CAPTURE_PROMO_BONUS         = 8000;
        internal const short SORTING_SCORE_KILLER_1           = 8000;
        internal const short SORTING_SCORE_KILLER_2           = 7000;
        internal const short SORTING_SCORE_KILLER_3           = 6000;
        internal const short SORTING_SCORE_HISTORY_MAX        = 5000;
        internal const short BAD_CAPTURE_PROMO_BONUS          = 1000;  
    }
}