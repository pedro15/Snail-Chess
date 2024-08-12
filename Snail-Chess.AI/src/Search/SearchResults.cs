using System;
using SnailChess.Core;

namespace SnailChess.AI.Search
{  
    public struct SearchResults
    {
        public int evaluation;
        public ulong nodesCount;
        public Move[] pv;
        public TimeSpan time;
        
        public SearchResults(int _evaluation , ulong _nodesCount, TimeSpan _time, Move[] _pv)
        {
            evaluation = _evaluation;
            nodesCount = _nodesCount;
            time = _time;
            pv = _pv;
        }
        
        public Move bestMove 
        {
            get
            {
                if (pv != null && pv.Length > 0) return pv[0];
                return 0;
            }
        }
    }
}