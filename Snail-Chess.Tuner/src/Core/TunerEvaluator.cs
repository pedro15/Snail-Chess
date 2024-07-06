using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SnailChess.Tuner.Data;
using SnailChess.Core;
using SnailChess.AI.Evaluation;

namespace SnailChess.Tuner.Core
{
    internal static class TunerEvaluator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddTunerParamFeature(ref Dictionary<string,TunerParamData> _params_db, in Dictionary<string,int> _indexes_db, 
            string _key, byte _color, int _multipler = 1)
        {
            int add_value = _color == Piece.White ? _multipler : -_multipler;
            if (_params_db.TryGetValue(_key, out TunerParamData current))
            {
                current.Count = (short)(current.Count + add_value);
                _params_db[_key] = current;
            }else if (_indexes_db.TryGetValue(_key, out int param_index)) 
            {
                _params_db.Add(_key, new TunerParamData((short)param_index, (short)add_value));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Evaluate(in TunerParam[] _params, in TunerParamData[] _params_data, int _gamePhase, byte _sideToMove)
        {
            int score_mg = 0, score_eg = 0, val;
            
            TunerParamData current_data;
            TunerParam current_param;
            
            for (int param_index = 0; param_index < _params_data.Length; param_index++)
            {
                current_data = _params_data[param_index];
                current_param = _params[current_data.ParamIndex];
                if (!current_param.IsValid) continue;
                
                val = current_param.value * current_data.Count;
                switch (current_param.phase)
                {
                    case PHASE_TYPE.GL:
                    score_mg += val;
                    score_eg += val;
                    break;

                    case PHASE_TYPE.MG:
                    score_mg += val;
                    break;

                    case PHASE_TYPE.EG:
                    score_eg += val;
                    break;
                }
            }
            
            int score = EvaluationUtils.InterpolateScores(score_mg, score_eg, _gamePhase);
            return _sideToMove == Piece.White ? score : -score;
        }
    }
}