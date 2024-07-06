namespace SnailChess.Tuner.Data
{
    internal struct TunerParamData
    {
        public readonly short ParamIndex;
        public short Count;
        
        internal TunerParamData(short _paramIndex, short _count)
        {
            ParamIndex = _paramIndex;
            Count = _count;
        }
    }
}