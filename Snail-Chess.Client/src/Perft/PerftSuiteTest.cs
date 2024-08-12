using System;

namespace SnailChess.Client.Perft
{
    [System.Serializable]
    public struct PerftSuiteTest
    {
        public string fen;
        public Tuple<int,ulong>[] expected;
        public PerftSuiteTest(string _fen , params Tuple<int,ulong>[] _expected)
        {
            fen = _fen;
            expected = _expected;
        }
        
    }
}