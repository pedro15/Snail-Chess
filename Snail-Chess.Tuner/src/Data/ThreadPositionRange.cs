namespace SnailChess.Tuner.Data
{
    internal readonly struct ThreadPositionRange 
    {
        public readonly int start;
        public readonly int end;
        
        public ThreadPositionRange(int _start, int _end)
        {
            start = _start;
            end = _end;
        }
    }
}