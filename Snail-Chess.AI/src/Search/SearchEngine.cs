using System;
using System.Runtime.CompilerServices;
using SnailChess.Core;

using static SnailChess.AI.Search.SearchConstants;
using SnailChess.AI.Evaluation;
using SnailChess.AI.HashTables;
using SnailChess.Core.MoveGen;

namespace SnailChess.AI.Search
{
    public sealed class SearchEngine
    {
        /* ---------------------------- 
            Events
        ------------------------------- */

        public delegate void d_OnSearchUpdated(int _depth, ulong _nodes, int _score, bool _mate, uint _speed , uint _time, Move[] _pv);
        public event d_OnSearchUpdated OnSearchUpdated;

        /* ------------------------------
            Triangular PV table
		---------------------------------
			PV line: e2e4 e7e5 g1f3 b8c6
		---------------------------------
			   0    1    2    3    4    5
      
	     0    m1   m2   m3   m4   m5   m6
      
         1    0    m2   m3   m4   m5   m6 
      
         2    0    0    m3   m4   m5   m6
      
         3    0    0    0    m4   m5   m6
       
         4    0    0    0    0    m5   m6
      
         5    0    0    0    0    0    m6
		--------------------------------- */
        private readonly Move[][] pv_table;
        private readonly Move[] pv_line;
        private readonly byte[] pv_lenght;

        /* ---------------------------- 
            Time control data
        ------------------------------- */

        private SearchType tc_searchType = SearchType.None;
        private uint tc_limit = 0;
        private bool tc_abort = false;
        private bool tc_isSearching = false;
        public bool IsBusy => tc_isSearching;
        
        /* ---------------------------- 
            Search data
        ------------------------------- */

        private readonly EvaluationController s_evaluator = null;
        private readonly ChessBoard s_board = null;
        private DateTime s_startTime;
        private ulong s_totalNodes = 0;
        private byte s_ply = 0;
        
        private bool s_rootNode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_ply == 0;
        }

        private ulong[] s_repetitions = null;
        private int s_repetitionIndex = 0;

        // Hash tables
        private HashTableSearch s_hashtable = null;

        // history heuristic table
        // [color][from][to]
        private short[][][] s_history = null;
        // killer heuristic tables
        // [move_number][ply]
        private Move[][] s_killer_moves = null;

        // LMR reductions table
        // [depth][move_count]
        private int[][] s_lmr_reductions = null;

        /* ---------------------------- 
            Search options
        ------------------------------- */
        private bool op_ENABLE_QS = true;
        private bool op_ENABLE_DRAW_DETECTION = true;
        private bool op_ENABLE_PVS = true;
        private bool op_ENABLE_LMR = true;
        private bool op_ENABLE_NMP = true;
        private bool op_ENABLE_SEE_pruning = true;
        private bool op_ENABLE_LMP = true;
        private bool op_ENABLE_RAZORING = true;
        private bool op_ENABLE_RFP = true;
        private bool op_ENABLE_IIR = true;
        private bool op_ENABLE_CHECK_EXTENSION = true;

        /* ---------------------------- 
            Initialization
        ------------------------------- */

        public SearchEngine()
        {
            pv_line = new Move[BoardPosition.MAX_PLY];
            pv_lenght = new byte[BoardPosition.MAX_PLY];
            pv_table = new Move[BoardPosition.MAX_PLY][];
            for (int pvindex = 0; pvindex < BoardPosition.MAX_PLY; pvindex++) pv_table[pvindex] = new Move[BoardPosition.MAX_PLY];

            s_board = new ChessBoard();
            s_evaluator = new EvaluationController();
            s_repetitions = new ulong[ChessBoard.HISTORY_LENGHT];
            s_hashtable = new HashTableSearch();
            
            // init history
            s_history = new short[2][][];
            for (int player = Piece.Black; player <= Piece.White; player++)
            {
                s_history[player] = new short[64][];
                for (byte sq = 0; sq < 64; sq++) s_history[player][sq] = new short[64];
            }

            // init killers
            s_killer_moves = new Move[3][];
            for (int killerCounter = 0; killerCounter < s_killer_moves.GetLength(0); killerCounter++)
            {
                s_killer_moves[killerCounter] = new Move[BoardPosition.MAX_PLY];
            }

            // init LMR reductions
            s_lmr_reductions = new int[BoardPosition.MAX_PLY][];
            for (int depth_count = 0; depth_count < s_lmr_reductions.GetLength(0); depth_count++)
            {
                s_lmr_reductions[depth_count] = new int[Move.MAX_MOVES];
                for (int move_count = 0; move_count < Move.MAX_MOVES; move_count++)
                {
                    s_lmr_reductions[depth_count][move_count] = (int)(0.4 * Math.Log(Math.Min(depth_count,31)) + 1.057 * Math.Log(Math.Min(move_count,31)));
                }
            }
        }
        
        /* ---------------------------- 
            Board control API
        ------------------------------- */
        
        public void AbortSearch()
        {
            if (IsBusy) tc_abort = true;
        }
        
        public string BoardString()
        {
            return s_board.ToString();
        }

        public BoardPosition ExportPosition()
        {
            if (IsBusy) return BoardPosition.EmptyPosition();
            return s_board.ExportPosition();
        }

        /* ---------------------------- 
            Search API
        ------------------------------- */
        
        public void AINewGame()
        {
            // clear hash table
            s_hashtable.Clear();
        }
        
        public void AILoadParams(in SearchOptions _searchOptions, in EvaluationParams _evalParams)
        {
            // set search options
            op_ENABLE_QS = _searchOptions.ENABLE_QS;
            op_ENABLE_DRAW_DETECTION = _searchOptions.ENABLE_DRAW_DETECTION;
            op_ENABLE_PVS = _searchOptions.ENABLE_PVS;
            op_ENABLE_LMR = _searchOptions.ENABLE_LMR;
            op_ENABLE_NMP = _searchOptions.ENABLE_NMP;
            op_ENABLE_SEE_pruning = _searchOptions.ENABLE_SEE_pruning;
            op_ENABLE_LMP = _searchOptions.ENABLE_LMP;
            op_ENABLE_RAZORING = _searchOptions.ENABLE_RAZORING;
            op_ENABLE_RFP = _searchOptions.ENABLE_RFP;
            op_ENABLE_IIR = _searchOptions.ENABLE_IIR;
            op_ENABLE_CHECK_EXTENSION = _searchOptions.ENABLE_CHECK_EXTENSION;
            // set eval params
            s_evaluator.LoadParams(in _evalParams);
        }

        public void AILoadPosition(in BoardPosition _position)
        {
            s_board.LoadPosition(in _position);
            Array.Clear(s_repetitions, 0, s_repetitions.Length);
            s_repetitionIndex = 0;
            s_repetitions[s_repetitionIndex] = s_board.key;
        }

        public bool AIMakeMove(in Move _move)
        {
            if (s_board.MakeMove(in _move))
            {
                s_repetitionIndex++;
                s_repetitions[s_repetitionIndex] = s_board.key;
                return true;
            }
            return false;
        }

        public bool AIUndoLastMove()
        {
            if (s_board.UndoLastMove())
            {
                s_repetitionIndex--;
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SearchResults FindBestMove(uint _wtime, uint _winc, uint _btime, uint _binc)
        {
            static uint filterTime(uint _time, uint _increment)
            {
                return (_time / 20) + (_increment / 2);   
            }

            uint time;
            if (s_board.sideToMove == Piece.White)
                time = filterTime(_wtime, _winc);
            else 
                time = filterTime(_btime, _binc);
            
            return FindBestMove(SearchType.FixedTime, time);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SearchResults FindBestMove(SearchType _searchType, uint _time_value)
        {
            // set time controls
            tc_searchType = _searchType;
            tc_limit = _time_value;
            tc_abort = false;

            // clear PV
            Array.Clear(pv_line, 0, pv_line.Length);
            Array.Clear(pv_lenght, 0 , pv_lenght.Length);
            for (int pvindex = 0; pvindex < BoardPosition.MAX_PLY; pvindex++)
                Array.Clear(pv_table[pvindex], 0, BoardPosition.MAX_PLY);

            // clear history
            for (int player = Piece.Black; player <= Piece.White; player++)
            {
                for (byte sq = 0; sq < 64; sq++) Array.Clear(s_history[player][sq], 0, 64);
            }
            // clear killers
            for (int killerCounter = 0; killerCounter < s_killer_moves.GetLength(0); killerCounter++)
            {
                Array.Clear(s_killer_moves[killerCounter], 0, BoardPosition.MAX_PLY);
            }
            
            // reset & load search params
            s_startTime = DateTime.UtcNow;
            s_ply = 0;
            s_totalNodes = 0;

            // do search !
            Move[] search_pv;

            _time_value = Math.Max(_time_value, 1);
            int eval = SearchRoot((_searchType == SearchType.FixedDepth) ? (int)Math.Min(BoardPosition.MAX_PLY, _time_value) : BoardPosition.MAX_PLY, out search_pv);
            
            TimeSpan elapsed = DateTime.UtcNow - s_startTime;
            return new SearchResults(eval, s_totalNodes, elapsed, search_pv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsTimeOver()
        {
            return (tc_searchType == SearchType.FixedNodes && s_totalNodes >= tc_limit) || 
                   (tc_searchType == SearchType.FixedTime && (DateTime.UtcNow - s_startTime).TotalMilliseconds >= tc_limit); 
        }

        /* ---------------------------- 
            Draw Detection
        ------------------------------- */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsRepetition()
        {
            for (int i = 0; i < s_repetitionIndex; i++)
            {
                if (s_repetitions[i] == s_board.key)
                {
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Is50MoveDraw()
        {
            if (s_board.halfMoves < 100) return false;
            if (!s_board.IsKingChecked()) return true;

            Span<Move> move_list = stackalloc Move[Move.MAX_MOVES];
            short target_index = s_board.GenerateMoves(in move_list);
            for (short move_index = 0; move_index <= target_index; move_index++)
            {
                if (s_board.MakeMove(in move_list[move_index]))
                {
                    s_board.UndoLastMove();   
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMaterialDraw()
        {
            byte piece_count = BitUtils.Count(s_board.Occupancy);
            
            // king vs king
            if (piece_count == 2) return true;
            
            // king vs minor piece (Knight or Bishop)
            if (piece_count == 3 && (BitUtils.Count(s_board.bitboards[Piece.Knight]) == 1 || 
                BitUtils.Count(s_board.bitboards[Piece.Bishop]) == 1))
                return true;
            
            // 4 piece only on the board
            if (piece_count == 4)
            {
                // (King + Knight + Knight VS King) , (King + Knight VS King + Knight)
                if (BitUtils.Count(s_board.bitboards[Piece.Knight]) == 2)
                    return true;
                // (king + Bishop vs King + Bishop)
                else if (BitUtils.Count(s_board.bitboards[Piece.Bishop]) == 2 && 
                     BitUtils.Count(s_board.bitboards[Piece.Bishop] & s_board.bitboards[Piece.Black]) == 1)
                    return true;
            }else if (piece_count == 5)
            {
                // (king + Bishop + Bishop vs King + Bishop)
                if (BitUtils.Count(s_board.bitboards[Piece.Bishop]) == 3 && 
                    (BitUtils.Count(s_board.bitboards[Piece.Bishop] & s_board.bitboards[Piece.Black]) == 1 || 
                     BitUtils.Count(s_board.bitboards[Piece.Bishop] & s_board.bitboards[Piece.White]) == 1))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsDraw()
        {
            return !s_rootNode && (IsRepetition() || Is50MoveDraw() || IsMaterialDraw());
        }

        /* ---------------------------- 
            Move Ordering
        ------------------------------- */


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EstimateGain(in Move _move)
        {
            if (_move.IsQuiet) 
                return 0;
            if (_move.flag == MoveFlag.EnPassant)
                return SEE_PIECE_VALUES[Piece.Pawn];
            
            int result = SEE_PIECE_VALUES[s_board.PieceOn(_move.to)];
            if (_move.IsPromotion)
                result += SEE_PIECE_VALUES[_move.promotionType] - SEE_PIECE_VALUES[Piece.Pawn];
                
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SEE(in Move _move, int _threshold)
        {
            byte from = _move.from, to = _move.to , next_victim, see_color;
            ulong occ, bishops, rooks, attackers, my_attackers, mask_attacker = 0UL;
            int see_balance;

            // balance is the value of the move minus threshold
            see_balance = EstimateGain(in _move) - _threshold;

            // best case still fails to beat the treshold
            if (see_balance < 0) return false;

            // next victim is moved piece or promotion type
            next_victim = _move.IsPromotion ? _move.promotionType : s_board.PieceOn(from);
            // worst case is losing the moved piece
            see_balance -= SEE_PIECE_VALUES[next_victim];

            // If the balance is positive even if losing the moved piece
            // the exchange is guaranteed to beat the threshold
            if (see_balance >= 0) return true;

            // lets suppose that the move was actually made
            occ = (s_board.Occupancy ^ (1UL << from)) | (1UL << to);
            if (_move.flag == MoveFlag.EnPassant && s_board.ep_square != 0) occ ^= 1UL << s_board.ep_square;
            
            // Get all pieces which attack the target square. And with current occupancy
            // so that we do not let the same piece attack twice
            attackers = s_board.GetAllAttackersTo(to, occ) & occ;
            
            // grab sliders for updating revealed attackers
            bishops = s_board.bitboards[Piece.Bishop] | s_board.bitboards[Piece.Queen];
            rooks   = s_board.bitboards[Piece.Rook]   | s_board.bitboards[Piece.Queen];

            // now it's our opponent turn to recapture
            see_color = Piece.FlipColor(s_board.sideToMove);


            while(true)
            {
                // if we do not have attackers left, we lose
                my_attackers = attackers & s_board.bitboards[see_color];
                if (my_attackers == 0UL) break;

                // Find our weakest piece to attack with
                for (next_victim = Piece.Pawn; next_victim <= Piece.King; next_victim++)
                {
                    mask_attacker = my_attackers & s_board.bitboards[next_victim];
                    if (mask_attacker > 0UL) break;
                }

                // Remove this attacker from the occupancy
                occ ^= BitUtils.GetLsb(mask_attacker);

                // A diagonal move may reveal bishop or queen attackers
                if (next_victim == Piece.Pawn || next_victim == Piece.Bishop || next_victim == Piece.Queen)
                    attackers |= MoveGenerator.BishopMoves(in occ, to) & bishops;
                
                // A vertical or horizontal move may reveal Rook or Queen attackers
                if (next_victim == Piece.Rook || next_victim == Piece.Queen)
                    attackers |= MoveGenerator.RookMoves(in occ, to) & rooks;
                
                // make sure that we did not add any already used attacks
                attackers &= occ;

                // swap the turn
                see_color = Piece.FlipColor(see_color);
                
                // Negamax the balance and add the value of the next victim
                see_balance = -see_balance -1 -SEE_PIECE_VALUES[next_victim];

                // if the balance is non negative after giving away our piece then we win
                if (see_balance >= 0)
                {
                    // if our last attacking piece is a king, and our opponent still has attackers, then we've
                    // lost as the move we followed would be illegal
                    if (next_victim == Piece.King && (attackers & s_board.bitboards[see_color]) != 0UL)
                    {
                        see_color = Piece.FlipColor(see_color);
                    }
                    break;
                }
            }

            // side to move after the loop loses
            return s_board.sideToMove != see_color;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MoveHasGoodPastHistory(in Move _move)
        {
            // move has good past history
            int history_value = s_history[s_board.sideToMove][_move.from][_move.to];
            if (history_value >= SORTING_SCORE_HISTORY_MAX / 3)
                return true;

            return false;                
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsKillerMove(in Move _move)
        {
            return s_killer_moves[0][s_ply] == _move 
                || s_killer_moves[1][s_ply] == _move  
                || s_killer_moves[2][s_ply] == _move;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void KillersUpdate(in Move _move)
        {
            s_killer_moves[2][s_ply] = s_killer_moves[1][s_ply];
            s_killer_moves[1][s_ply] = s_killer_moves[0][s_ply];
            s_killer_moves[0][s_ply] = _move; 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HistoryUpdate(in Move _move, int _depth)
        {
            short h_bonus = (short)(_depth * _depth);
            short h_value = s_history[s_board.sideToMove][_move.from][_move.to];

            // add bonus to history value
            h_value += (short)(h_bonus - h_value * Math.Abs(h_bonus) / 256);

            // clamp to max story value
            h_value = Math.Min(h_value,  SORTING_SCORE_HISTORY_MAX);
            h_value = Math.Max(h_value, (short)-SORTING_SCORE_HISTORY_MAX); 

            s_history[s_board.sideToMove][_move.from][_move.to] = h_value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short ScoreMove(in Move _move, in Move _ttMove)
        {
            // ======== [ Score TT Moves] ====================
            // TT moves are the highest priority, so return inmediately
            if (_ttMove && _ttMove == _move) return SORTING_SCORE_TTMOVE;

            if (_move.IsQuiet)
            {
                // ======== [ Score quiet moves] ====================
                // first killer
                if (_move == s_killer_moves[0][s_ply])
                    return SORTING_SCORE_KILLER_1;
                // second killer
                else if (_move == s_killer_moves[0][s_ply])
                    return SORTING_SCORE_KILLER_2;
                // third killer
                else if (_move == s_killer_moves[1][s_ply])
                    return SORTING_SCORE_KILLER_3;
                // score history
                else 
                    return s_history[s_board.sideToMove][_move.from][_move.to];

            }else 
            {
                short loud_score = 0;

                // ======== [ Score Loud moves] ====================    
                // captures
                if (_move.IsCapture)
                {
                    byte attacker_piece = s_board.PieceOn(_move.from);
                    byte victim_piece = _move.flag == MoveFlag.EnPassant ? Piece.Pawn : s_board.PieceOn(_move.to);

                    if (SEE(in _move, -SEE_PIECE_VALUES[Piece.Pawn]))
                    {
                        if (_move.IsPromotion) 
                            loud_score += GOOD_CAPTURE_PROMO_BONUS;
                        
                        loud_score += (short)(MVV_LVA[attacker_piece][victim_piece] + SORTING_SCORE_GOOD_CAPTURES);
                    }else 
                    {
                        if (_move.IsPromotion)
                            loud_score += BAD_CAPTURE_PROMO_BONUS;
                        
                        loud_score += MVV_LVA[attacker_piece][victim_piece];
                    }
                }
                // promotion
                else if (_move.IsPromotion)
                {
                    return SORTING_SCORE_PROMOTION;
                }

                return loud_score;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScoreMoveList(in Span<Move> _moves, ref Span<short> _moveScores, short _targetIndex, in Move _ttMove)
        {
            for (short move_index = 0; move_index <= _targetIndex; move_index++)
            {
                _moveScores[move_index] = ScoreMove(in _moves[move_index], in _ttMove);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SortMoves(ref Span<Move> _moves, ref Span<short> _moveScores, short _currentIndex, short _targetIndex)
        {
            for (short move_index = _currentIndex; move_index <= _targetIndex; move_index++)
            {
                if (_moveScores[move_index] >= _moveScores[_currentIndex])
                {
                    (_moves[_currentIndex], _moves[move_index]) = (_moves[move_index],_moves[_currentIndex]);
                    (_moveScores[_currentIndex], _moveScores[move_index]) = (_moveScores[move_index],_moveScores[_currentIndex]);
                }
            }
        }

        /* ---------------------------- 
            Search
        ------------------------------- */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int SearchRoot(int _depth, out Move[] _pv)
        {
            int alpha = -VALUE_INFINITY;
            int beta  =  VALUE_INFINITY;
            
            tc_isSearching = true;
            int score = 0;

            Move[] search_pv = new Move[0];
            _pv =  search_pv;

            // Iterative deeping
            int current_depth = 1;
            while (current_depth <= _depth)
            {
                score = Search(alpha, beta, current_depth);
                
                // forced stop, this search is not valid
                if (tc_abort)
                {
                    break;
                }

                if (score <= alpha || score >= beta)
                {
                    // fell outside the window, so try again with a full-width window (and the same depth)
                    alpha = -VALUE_INFINITY;
                    beta  =  VALUE_INFINITY;
                    continue;
                }

                // collect search data
                float etime = Convert.ToSingle((DateTime.UtcNow - s_startTime).TotalMilliseconds);
                if (etime <= 0) etime = 0.01f;

                uint speed = Convert.ToUInt32(s_totalNodes / (etime/1000));
                uint time = Convert.ToUInt32(etime);

                // check pv
                if (pv_lenght[0] > 0)
                {
                    for (int pvindex = 0; pvindex < pv_lenght[0]; pvindex++)
                        pv_line[pvindex] = pv_table[0][pvindex];
                    
                    search_pv = new Move[pv_lenght[0]];
                    Array.Copy(pv_line, search_pv, pv_lenght[0]);
                    _pv = search_pv;
                    
                    if (score >= -VALUE_MATE && score < -VALUE_MATE_SCORE)
                        OnSearchUpdated?.Invoke(current_depth, s_totalNodes, -(score + VALUE_MATE) / 2 - 1, true, speed, time, search_pv);
                    else if (score > VALUE_MATE_SCORE && score < VALUE_MATE)
                        OnSearchUpdated?.Invoke(current_depth, s_totalNodes, (VALUE_MATE - score) / 2 + 1, true, speed, time, search_pv);
                    else 
                        OnSearchUpdated?.Invoke(current_depth, s_totalNodes, score, false, speed, time, search_pv);
                }

                // set up the window for the next iteration
                alpha = score - 50;
                beta = score + 50;
                
                // increase depth for next iteration
                current_depth++;
                // reset ply for next iteration
                s_ply = 0;
            }
            
            tc_isSearching = false;
            return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanApplyLMR(in Move _move, in Move _ttMove)
        {
            // Do not apply for: TT move
            if (_ttMove && _move == _ttMove) 
                return false;
            
            // Do not apply for: Noisy moves (promotion,capture,castle)
            if (!_move.IsQuiet) return false;

            // Do not apply for: good quiets
            if (IsKillerMove(in _move) || MoveHasGoodPastHistory(in _move)) return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BoardHasNonPawns()
        {
            ulong color_bb = s_board.bitboards[s_board.sideToMove];
            return (color_bb  ^ ( color_bb & s_board.bitboards[Piece.Pawn]) ^ ( color_bb & s_board.bitboards[Piece.King])) != 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Quiescence(int _alpha, int _beta)
        {
            // stop at draw or time over
            if (tc_abort || IsDraw()) return 0;
            // increase nodes
            s_totalNodes++;
            // current static eval
            int eval = s_evaluator.Evaluate(in s_board);
            // we're too deep or QS disabled
            if (!op_ENABLE_QS || s_ply >= BoardPosition.MAX_PLY -1) return eval;
            // validate eval score
            if (eval >= _beta) return _beta;
            else if (eval > _alpha) _alpha = eval;
            // side to move in-check?
            bool in_check = s_board.IsKingChecked();

            int score;
            Move current_move;
            Span<Move> moves = stackalloc Move[Move.MAX_MOVES];
            Span<short> move_socores = stackalloc short[Move.MAX_MOVES];

            short target_index = s_board.GenerateMoves(in moves, false);
            ScoreMoveList(in moves, ref move_socores, target_index, Move.NO_MOVE);

            for (short move_index = 0; move_index <= target_index; move_index++)
            {
                SortMoves(ref moves, ref move_socores, move_index, target_index);
                current_move = moves[move_index];

                if (op_ENABLE_SEE_pruning && !in_check && move_socores[move_index] < SORTING_SCORE_KILLER_3)
                {
                    continue;
                }

                if (s_board.MakeMove(in current_move))
                {
                    s_ply++;
                    score = -Quiescence(-_beta, -_alpha);
                    s_board.UndoLastMove();
                    s_ply--;
                    
                    if (score > _alpha)
                    {
                        _alpha = score;
                        if (score >= _beta) return _beta;
                    }
                }
            }

            return _alpha;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Search(int _alpha, int _beta, int _depth)
        {
            // extract pv data
            pv_lenght[s_ply] = s_ply;
            bool pv_node = _beta - _alpha > 1;

            // check for time over
            if (tc_abort) 
                return 0;
            else if (_depth > 0 && s_totalNodes % 2048 == 0 && IsTimeOver())
            {
                tc_abort = true;
                return 0;
            }

            // avoid obvious draws
            if (IsDraw()) 
                return 0;
            
            // we're too deep
            if (s_ply >= BoardPosition.MAX_PLY -1) 
                return s_evaluator.Evaluate(in s_board);

            /* ==================================== *
            * - Transposition table              
            * ===================================== */
            HashTableFlags tt_flag = HashTableFlags.ALPHA;
            HashEntrySearch tt_entry = s_hashtable.GetEntry(s_board.key);
            if (!pv_node && tt_entry)
            {
                if (tt_entry.depth >= _depth)
                {
                    int tt_score = tt_entry.score;

                    if (tt_score < -VALUE_MATE_SCORE) tt_score += s_ply;
                    if (tt_score > VALUE_MATE_SCORE)  tt_score -= s_ply;

                    if (tt_entry.flags == HashTableFlags.EXACT)
                        return tt_score;
                    if (tt_entry.flags == HashTableFlags.ALPHA && tt_score <= _alpha)
                        return _alpha;
                    if (tt_entry.flags == HashTableFlags.BETA && tt_score >= _beta)
                        return _beta;
                }
            }
            
            /* ====================================== *
            * - Check Extension
            * if we are in check we can try to look further 
            * so we can try to avoid checkmate
            ========================================= */
            bool in_check = s_board.IsKingChecked();
            if (op_ENABLE_CHECK_EXTENSION && in_check) _depth++;

            // recursion escape
            if (_depth < 1) return Quiescence(_alpha, _beta);
            
            // increase nodes
            s_totalNodes++;
            // current node score
            int score = 0;
            // current node static evaluation
            int eval = tt_entry ? tt_entry.evaluation : s_evaluator.Evaluate(in s_board);

            /* ==================================== *
            * - IIR: Internal Iteractive reductions              
            * ===================================== */
            if (op_ENABLE_IIR && !tt_entry && _depth > 4) _depth--;

            if (!pv_node && !in_check)
            {
                /* ==================================== *
                * - Reverse Futility Pruning              
                * ===================================== */
                if (op_ENABLE_RFP && _depth < 7 && eval - (70 * _depth) >= _beta) return eval;

                /* ==================================== *
                * - Null move pruning              
                * ===================================== */
                if (op_ENABLE_NMP && _depth >= 3)
                {
                    // Make null move
                    s_board.MakeNullMove();
                    s_ply++;
                    s_repetitionIndex++;
                    s_repetitions[s_repetitionIndex] = s_board.key;

                    // Search at a lower depth
                    score = -Search(-_beta, 1 -_beta, _depth - 2 - 1);

                    // Undo null move
                    s_board.UndoLastMove();
                    s_ply--;
                    s_repetitionIndex--;
                    
                    // check for time over
                    if (tc_abort) return 0;

                    // node fails high
                    if (score >= _beta) return _beta;
                }

                /* ==================================== *
                * - Razoring             
                * ===================================== */
                if (op_ENABLE_RAZORING && _depth <= 5 && eval + 256 * _depth < _alpha)
                {
                    int razor_score = Quiescence(_alpha, _beta);
                    if (razor_score <= _alpha)
                        return razor_score;
                }

            }

            bool skip_quiets = false;
            int legal_moves = 0, searched_moves = 0;
            Move current_move = Move.NO_MOVE, best_move = tt_entry.bestMove;
            Span<Move> moves = stackalloc Move[Move.MAX_MOVES];
            Span<short> move_scores = stackalloc short[Move.MAX_MOVES];

            short target_index = s_board.GenerateMoves(in moves);
            ScoreMoveList(in moves, ref move_scores, target_index, tt_entry.bestMove);
            
            for (short move_index = 0; move_index <= target_index; move_index++)
            {
                SortMoves(ref moves, ref move_scores, move_index, target_index);
                current_move = moves[move_index];
                
                if (skip_quiets && current_move.IsQuiet) continue;

                // Prune bad or late moves
                if (!in_check && _alpha > -VALUE_MATE_SCORE && move_scores[move_index] < SORTING_SCORE_KILLER_3)
                {
                    /* ==================================== *
                    * - SEE pruning              
                    * ===================================== */
                    if (op_ENABLE_SEE_pruning && searched_moves > 0 && _depth <= 8 && 
                        !SEE(in current_move, current_move.IsQuiet ? (-85 * _depth) : (-35 * _depth * _depth)))
                    {
                        continue;
                    }
                    
                    /* ==================================== *
                    * - Late Move pruning            
                    * ===================================== */
                    if (op_ENABLE_LMP && current_move.IsQuiet && _depth <= 5 && searched_moves >= 4 * _depth * _depth) 
                    {
                        skip_quiets = true;
                        continue;
                    }
                }
                
                if (s_board.MakeMove(in current_move))
                {
                    legal_moves++;
                    s_ply++;
                    s_repetitionIndex++;
                    s_repetitions[s_repetitionIndex] = s_board.key;

                    /* ======================================== *
                    *          [ Search Recursion ]             *
                    * ========================================= */

                    if (searched_moves == 0)
                    {
                        // normal alpha-beta search
                        score = -Search(-_beta, -_alpha, _depth - 1);
                    }else 
                    {
                        /* ==================================== *
                        * - LMR: Late move reductions              
                        * ===================================== */
                        if (op_ENABLE_LMR && searched_moves >= LMR_MOVE_COUNT && _depth >= LMR_DEPTH && !in_check && !pv_node && CanApplyLMR(in current_move, in tt_entry.bestMove))
                        {
                            score = -Search(-_alpha - 1, -_alpha, _depth - s_lmr_reductions[_depth][searched_moves]);
                        }else 
                        {
                            // ensure that we do a re-search at this point
                            score = _alpha + 1;
                        }
                        
                        if (score > _alpha)
                        {
                            /* ==================================== *
                            * - PVS: principal variation search            
                            * ===================================== */
                            if (op_ENABLE_PVS)
                            {
                                score = -Search(-_alpha - 1, -_alpha, _depth - 1);
                            }else 
                            {
                                score = _alpha + 1;
                            }

                            // failure check
                            if ((score > _alpha) && (score < _beta))
                            {
                                // re- search with normal alpha-beta 
                                score = -Search(-_beta , -_alpha, _depth - 1);
                            }
                        }
                    }

                    /* ======================================== *
                    *          [ Post-Search Recursion ]        *
                    * ========================================= */

                    s_board.UndoLastMove();
                    s_ply--;
                    s_repetitionIndex--;
                    searched_moves++;

                    // found a better move
                    if (score > _alpha)
                    {
                        // copy PV
                        pv_table[s_ply][s_ply] = current_move;
                        for (int next_ply = s_ply + 1; next_ply < pv_lenght[s_ply + 1]; next_ply++) pv_table[s_ply][next_ply] = pv_table[s_ply + 1][next_ply];
                        pv_lenght[s_ply] = pv_lenght[s_ply + 1];

                        // update score
                        _alpha = score;
                        tt_flag = HashTableFlags.EXACT;
                        best_move = current_move;

                        // beta cut-off
                        if (score >= _beta)
                        {
                            if (current_move.IsQuiet)
                            {
                                // History heuristic update
                                HistoryUpdate(in current_move, _depth);
                                // Killer heuristic update
                                KillersUpdate(in current_move);
                            }

                            // TT record entry
                            tt_entry.evaluation = (short)eval;
                            tt_entry.score = (short)_beta;
                            tt_entry.depth = (byte)_depth;
                            tt_entry.key = s_board.key;
                            tt_entry.flags = HashTableFlags.BETA;
                            tt_entry.bestMove = best_move;

                            s_hashtable.RecordEntry(tt_entry, s_ply);
                            return _beta;
                        }
                    }
                }
            }

            // we don't have any legal moves to make in the current postion
            if (legal_moves == 0)
            {
                if (in_check)
                {
                    return -VALUE_MATE + s_ply;
                }
                else 
                {
                    return 0;
                }
            }
            
            // TT record entry
            tt_entry.evaluation = (short)eval;
            tt_entry.score = (short)_alpha;
            tt_entry.depth = (byte)_depth;
            tt_entry.key = s_board.key;
            tt_entry.flags = tt_flag;
            tt_entry.bestMove = best_move;

            s_hashtable.RecordEntry(tt_entry, s_ply);
            return _alpha;
        }
    }
}