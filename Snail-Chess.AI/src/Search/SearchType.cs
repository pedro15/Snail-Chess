namespace SnailChess.AI.Search
{
    public enum SearchType : byte
    {
        None = 0,
        FixedDepth = 1,
        FixedTime =  2,
        FixedNodes = 4,
    }
}