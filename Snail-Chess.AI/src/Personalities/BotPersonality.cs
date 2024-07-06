using SnailChess.AI.Evaluation;
using SnailChess.AI.Search;

namespace SnailChess.AI.Personalities
{
    [System.Serializable]
    public partial struct BotPersonality
    {
        public SearchOptions searchOptions;
        public EvaluationParams evaluationParams;
    }
}