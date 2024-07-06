namespace SnailChess.Tuner.Data
{
    internal enum PHASE_TYPE : byte
    {
        /// <summary>
        /// Middle-Game Phase
        /// </summary>
        MG  = 1,
        /// <summary>
        /// End-Game Phase
        /// </summary>
        EG  = 2,
        /// <summary>
        /// Global Phase
        /// </summary>
        GL  = 3,
    }

    internal enum PARAM_TYPE : int 
    {
        NONE = 0,
        DEFAULT = 1,   
        ARRAY = 2,
        PST = 3
    }

    internal struct TunerParam
    {
        public readonly PARAM_TYPE paramType;
        public readonly PHASE_TYPE phase;
        public readonly string name;
        public readonly int array_index;
        public int value;

        public TunerParam(PARAM_TYPE _paramType, PHASE_TYPE _phase, string _name,int _value, int _array_index = -1)
        {
            paramType = _paramType;
            phase = _phase;
            name = _name;
            value = _value;
            array_index = _array_index;
        }
        
        public bool IsValid => paramType != PARAM_TYPE.NONE;
        public bool Match(string _key, PARAM_TYPE _paramType) => paramType == _paramType && name.Contains(_key);
    }
}