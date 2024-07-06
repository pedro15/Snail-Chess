using SnailChess.Tuner.Data;

namespace SnailChess.Tuner
{
    internal readonly struct TunerPosition 
    {
        public readonly string fen;
        public readonly float outcome;
        public readonly byte gamePhase;
        public readonly byte sideToMove;
        public readonly TunerParamData[] params_data;

        public TunerPosition(string _fen, float _outcome, byte _gamePhase, byte _sideToMove, params TunerParamData[] _params_data)
        {
            fen = _fen;
            outcome = _outcome;
            gamePhase = _gamePhase;
            sideToMove = _sideToMove;
            params_data = _params_data;
        }

        
    }
}