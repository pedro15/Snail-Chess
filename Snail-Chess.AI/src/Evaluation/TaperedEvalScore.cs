namespace SnailChess.AI.Evaluation
{
    internal struct TaperedEvalScore
    {
        public short score_mg;
        public short score_eg;

        public TaperedEvalScore(short _score_mg, short _score_eg)
        {
            score_mg = _score_mg;
            score_eg = _score_eg;
        }
    }
}