namespace SnailChess.AI.Search
{
    [System.Serializable]
    public readonly struct SearchOptions
    {
        public readonly bool ENABLE_QS;
        public readonly bool ENABLE_DRAW_DETECTION;
        public readonly bool ENABLE_PVS;
        public readonly bool ENABLE_LMR;
        public readonly bool ENABLE_NMP;
        public readonly bool ENABLE_SEE_pruning;
        public readonly bool ENABLE_LMP;
        public readonly bool ENABLE_RAZORING;
        public readonly bool ENABLE_RFP;
        public readonly bool ENABLE_IIR;
        public readonly bool ENABLE_CHECK_EXTENSION;

        public SearchOptions(bool _enable_qs, bool _enable_draw_detection, bool _enable_pvs, bool _enable_lmr, bool _enable_nmp, bool _enable_see_pruning, 
            bool _enable_lmp, bool _enable_razoring, bool _enable_rfp, bool _enable_iir, bool _enable_check_extension)
        {
            ENABLE_QS = _enable_qs;
            ENABLE_DRAW_DETECTION = _enable_draw_detection;
            ENABLE_PVS = _enable_pvs;
            ENABLE_LMR = _enable_lmr;
            ENABLE_NMP = _enable_nmp;
            ENABLE_SEE_pruning =  _enable_see_pruning;
            ENABLE_LMP = _enable_lmp;
            ENABLE_RAZORING = _enable_razoring;
            ENABLE_RFP = _enable_rfp;
            ENABLE_IIR = _enable_iir;
            ENABLE_CHECK_EXTENSION  = _enable_check_extension;
        }
    }
}